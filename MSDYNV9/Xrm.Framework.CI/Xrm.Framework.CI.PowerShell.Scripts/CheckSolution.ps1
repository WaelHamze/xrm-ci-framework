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
[string]$Geography, #The regional endpoint to hit
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
	TenantId = "$TenantId"
	ClientApplicationId = "$ApplicationId"
	ClientApplicationSecret = ConvertTo-SecureString "$ApplicationSecret" -AsPlainText -Force
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