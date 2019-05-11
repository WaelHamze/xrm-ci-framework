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
[string]$AzureADPath, #The full path to the Azure AD PowerShell Module
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
Write-Verbose "AzureADPath = $AzureADPath"
Write-Verbose "PowerAppsCheckerPath = $PowerAppsCheckerPath"

Write-Verbose "Importing Azure AD: $AzureADPath"
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

$resultFile = Get-ChildItem -Path "$OutputPath" -Filter "*.zip" | Sort CreationTime -Descending | Select -First 1

if ($resultFile)
{
	Add-Type -assembly "System.IO.Compression.FileSystem"

	try
	{
		$resultsZip = [io.compression.zipfile]::OpenRead("$($resultFile.FullName)")

		$jsonFile = $resultsZip.Entries | Where-Object { $_.Name -ilike "*.sarif"}

		if ($jsonFile)
		{
			try
			{
				$reader = New-Object -TypeName System.IO.StreamReader -ArgumentList $jsonFile.Open()
				$rawContent = $reader.ReadToEnd()

				$jsonContent = $rawContent | ConvertFrom-Json

				if ($jsonContent.runs.length -ne 1)
				{
					throw "runs count doesn't equal 1"
				}

				$results = $jsonContent.runs[0].results

				Write-Host "============================================" -ForegroundColor DarkGreen
				Write-Host "Total number of issues detected: $($results.length)" -ForegroundColor DarkGreen
				Write-Host "============================================" -ForegroundColor DarkGreen

				$high = @($jsonContent.runs[0].results | Where-Object { $_.properties.engineseverity -eq "High"})
				$medium = @($jsonContent.runs[0].results | Where-Object { $_.properties.engineseverity -eq "Medium"})
				$low = @($jsonContent.runs[0].results | Where-Object { $_.properties.engineseverity -eq "Low"})
				
				Write-Host "High:   $($high.length)" -ForegroundColor Green
				Write-Host "Medium: $($medium.length)" -ForegroundColor Green
				Write-Host "Low:    $($low.length)" -ForegroundColor Green
				Write-Host "============================================" -ForegroundColor DarkGreen

				if ($EnableThresholds)
				{
					$breached = (
								($high.Length -gt $HighThreshold) -or
								($medium.Length -gt $MediumThreshold) -or
								($low.Length -gt $LowThreshold)
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
			finally
			{
				if ($reader)
				{
					Write-Verbose "Disposing of reader"
					$reader.Dispose()
					Write-Verbose "Reader Disposed"
				}
			}
		}
	}
	finally
	{
		if ($resultsZip)
		{
			Write-Verbose "Disposing of resultZip"
			$resultsZip.Dispose()
			Write-Verbose "resultsZip Disposed"
		}
	}
}
else
{
	throw "Couldn't find PowerApps Checker result file"
}

# End of script

Write-Verbose 'Leaving CheckSolution.ps1'