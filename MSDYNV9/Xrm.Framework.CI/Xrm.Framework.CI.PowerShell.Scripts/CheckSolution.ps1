#
# Filename: CheckSolution.ps1
#
[CmdletBinding()]

param(
[string]$SolutionFile, #The absolute path to the solution file zip to be imported
[string]$OutputPath, #The full path to where you want results to be stored
[string]$TenantId , #The tenant Id where your instance resides
[string]$ApplicationId , #The application Id used for the connection
[string]$ApplicationSecret , #The application secret used for connection
[string]$Ruleset , #The ruleset to be used when checking
[string[]]$RuleCodes, #The Ids of the rules used when checking
[string[]]$ExcludedFiles, #The Files/Patterns to be excluded from scanning
[string]$RuleOverridesJson, #Json string containing rule overrides
[string]$Geography, #The regional endpoint to hit
[string]$LocaleName, #Specifies the language code that determines how the results are listed, such as es, for Spanish. The languages that are supported are included in the validation set of the parameter. 
[int]$MaxStatusChecks, #Maximum number of times in which status calls are made to the service. The default number is 20, which in most cases is sufficient. If the threshold is exceeded, then a timeout exception is thrown.
[int]$SecondsBetweenChecks, #The number of seconds between status checks. The default is 15 seconds.
[int]$MaxConnectionTimeOutMinutes, #Maximum number in minutes to wait before quitting a web based operation.
[string]$PowerAppsCheckerPath, #The full path to the PowerApp Checker PowerShell Module
[bool]$EnableThresholds, #Enables threshold checks
[string]$ThresholdAction, # Warn, Error - The type of action to generate when number of issues exceeds threshold limit
[int]$CriticalThreshold, #Number of critical severity issues
[int]$HighThreshold, #Number of high severity issues
[int]$MediumThreshold, #Number of medium severity issues
[int]$LowThreshold #Number of low severity issues
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering CheckSolution.ps1'

#Print Parameters

Write-Verbose "SolutionFile = $SolutionFile"
Write-Verbose "OutputPath = $OutputPath"
Write-Verbose "TenantId = $TenantId"
Write-Verbose "ApplicationId = $ApplicationId"
Write-Verbose "Ruleset = $Ruleset"
Write-Verbose "RuleCodes = $RuleCodes"
Write-Verbose "ExcludedFiles = $ExcludedFiles"
Write-Verbose "RuleOverridesJson = $RuleOverridesJson"
Write-Verbose "Geography = $Geography"
Write-Verbose "PowerAppsCheckerPath = $PowerAppsCheckerPath"

Write-Verbose "Importing PowerApps Checker: $PowerAppsCheckerPath"
Import-module "$PowerAppsCheckerPath\Microsoft.PowerApps.Checker.PowerShell.psd1"

#Run PowerApps Checker

if ($Ruleset)
{
    $rulesets = Get-PowerAppsCheckerRulesets -Geography "$Geography"

    if ($rulesets.Length -gt 0)
    {
	    $rulesetToUse = $rulesets | where Name -EQ "$RuleSet"

	    if ($rulesetToUse)
	    {
		    Write-Verbose "Ruleset found"
	    }
	    else
	    {
		    throw "$RuleSet ruleset was not found"
	    }
    }
    else
    {
	    throw "No rule sets found"
    }
}
elseif ($RuleCodes)
{
    $rules = @()
    
    foreach($ruleCode in $RuleCodes)
    {
        $rule = New-Object -TypeName "Microsoft.PowerApps.Checker.Client.Models.Rule"
        $rule.Code = $ruleCode
        $rules += $rule
    }
}
else
{
    throw "Either RuleSet or RuleCodes is required"
}

$CheckParams = @{
	FileUnderAnalysis = "$SolutionFile"
	OutputDirectory = "$OutputPath"
	Geography = "$Geography"
	TenantId = "$TenantId"
	ClientApplicationId = "$ApplicationId"
	ClientApplicationSecret = ConvertTo-SecureString "$ApplicationSecret" -AsPlainText -Force
	LocaleName = "$LocaleName"
	MaxStatusChecks = $MaxStatusChecks
	SecondsBetweenChecks = $SecondsBetweenChecks
}
if ($rulesetToUse)
{
    $CheckParams.Ruleset = $rulesetToUse
}
if ($rules)
{
    $CheckParams.Rule = $rules
}

if ($ExcludedFiles)
{
    $CheckParams.ExcludedFileNamePattern = $ExcludedFiles
}

if ($RuleOverridesJson)
{
	$overrides = @()
	
	#load json string into array
	$array = ConvertFrom-Json $RuleOverridesJson

	Write-Verbose ("Adding ({0}) Rule Overrides" -f $array.Count)
	
	#iterate through the configuration items and set secure configuration
	For ($i=0; $i -lt $array.Count; $i++)
	{
		$ruleId = $array[$i][0]
		$level = $array[$i][1]
		
		Write-Host "Adding override : $ruleId $level"

		$override = New-PowerAppsCheckerRuleLevelOverride -Id $ruleId -OverrideLevel $level

		$overrides += $override
	}

	if ($overrides.Count -gt 0)
	{
		$CheckParams.RuleLevelOverrides = $overrides

		Write-Host ("Added ({0}) Rule Overrides" -f $overrides.Count)
	}
}

if ($MaxConnectionTimeOutMinutes -ne 0)
{
	$CheckParams.MaxConnectionTimeOutMinutes = $MaxConnectionTimeOutMinutes
}

$response = Invoke-PowerAppsChecker @CheckParams

if( $response)
{
	$CheckStatus = $response.Status

	Write-Host "PowerApps Check Status: $CheckStatus"
	Write-Host "Result File: $($response.DownloadedResultFiles)"
	Write-Host "Result File Uri: $($response.ResultFileUris)"

	if (($CheckStatus -eq "Finished") -or ($CheckStatus -eq "FinishedWithErrors"))
	{
		if ($CheckStatus -eq "FinishedWithErrors")
		{
			Write-Warning "PowerApp Checker finished with errors. Check results for more information"
		}

		Write-Host "============================================" -ForegroundColor DarkGreen

		$issues = $response.IssueSummary
		
		$critical = $issues.CriticalIssueCount
		$high = $issues.HighIssueCount
		$medium = $issues.MediumIssueCount
		$low = $issues.LowIssueCount
		$info = $issues.InformationalIssueCount
				
		Write-Host "Critical:   $critical" -ForegroundColor Green
		Write-Host "High:   $high" -ForegroundColor Green
		Write-Host "Medium: $medium" -ForegroundColor Green
		Write-Host "Low:    $low" -ForegroundColor Green
		Write-Host "Informational:    $info" -ForegroundColor Green
		Write-Host "============================================" -ForegroundColor DarkGreen

		$md = "| Severity | Count |  `r`n"
		$md += "|:-----------|:-----------:|  `r`n"
		$md += "| Critical | $critical |  `r`n"
		$md += "| High | $high |  `r`n"
		$md += "| Medium | $medium |  `r`n"
		$md += "| Low | $low |  `r`n"
		$md += "| Informational | $info |  `r`n"

		Set-Content -Path "$OutputPath\CheckerResultsSummary.md" -Value "$md"

		if ($EnableThresholds)
		{
			$breached = (
						($critical -gt $CriticalThreshold) -or
						($high -gt $HighThreshold) -or
						($medium -gt $MediumThreshold) -or
						($low -gt $LowThreshold)
						)
					
			if ($breached)
			{
				$msg = "Number of issues detected above threshold limits"
				if ($ThresholdAction -eq "Warn")
				{
					Write-Warning $msg
				}
				elseif ($ThresholdAction -eq "Error")
				{
					Write-Error $msg
				}
			}
		}
	}
	elseif ($CheckStatus -eq "Failed")
	{
		throw "PowerApps Checker Failed"
	}
	else
	{
		Write-Warning "Unknown status return from PowerApps Checker"
	}
}
else
{
	throw "Didn't recieve response from PowerApps Checker"
}

Write-Verbose 'Leaving CheckSolution.ps1'

# End of script