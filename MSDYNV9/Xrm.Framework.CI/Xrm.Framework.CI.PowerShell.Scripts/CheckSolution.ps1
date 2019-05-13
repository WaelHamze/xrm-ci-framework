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
[string]$RulesetId , #The ruleset to be used when checking
[string]$Geography, #The regional endpoint to hit
#[string]$AzureADPath, #The full path to the Azure AD PowerShell Module
[string]$PowerAppsCheckerPath, #The full path to the PowerApp Checker PowerShell Module
[bool]$EnableThresholds, #Enables threshold checks
[string]$ThresholdAction, # Warn, Error - The type of action to generate when number of issues exceeds threshold limit
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
Write-Verbose "ApplicationSecret = $ApplicationSecret"
Write-Verbose "RulesetId = $RulesetId"
Write-Verbose "Geography = $Geography"
#Write-Verbose "AzureADPath = $AzureADPath"
Write-Verbose "PowerAppsCheckerPath = $PowerAppsCheckerPath"

#Write-Verbose "Importing Azure AD: $AzureADPath"
#Import-module "$AzureADPath\AzureAD.psd1"

Write-Verbose "Importing PowerApps Checker: $PowerAppsCheckerPath"
Import-module "$PowerAppsCheckerPath\PowerApps.Checker.psd1"

#Run PowerApps Checker

$CheckParams = @{
	FileUnderAnalysis = "$SolutionFile"
	OutputDirectory = "$OutputPath"
	TenantId = "$TenantId"
	ApplicationId = "$ApplicationId"
	ApplicationSecret = "$ApplicationSecret"
	RulesetId = "$RulesetId"
	Geography = "$Geography"
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

		$high = $response.highIssueCount
		$medium = $response.mediumIssueCount
		$low = $response.lowIssueCount
				
		Write-Host "High:   $high" -ForegroundColor Green
		Write-Host "Medium: $medium" -ForegroundColor Green
		Write-Host "Low:    $low" -ForegroundColor Green
		Write-Host "============================================" -ForegroundColor DarkGreen

		if ($EnableThresholds)
		{
			$breached = (
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