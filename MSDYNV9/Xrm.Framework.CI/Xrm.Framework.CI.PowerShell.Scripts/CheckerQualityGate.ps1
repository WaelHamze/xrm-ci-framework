#
# Filename: CheckerQualityGate.ps1
#
[CmdletBinding()]

param(
[string]$ResultsFile, #The absolute path to the solution file zip to be imported
[string]$ThresholdAction, # Warn, Error - The type of action to generate when number of issues exceeds threshold limit
[int]$CriticalThreshold, #Number of critical severity issues
[int]$HighThreshold, #Number of high severity issues
[int]$MediumThreshold, #Number of medium severity issues
[int]$LowThreshold #Number of low severity issues
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering CheckerQualityGate.ps1'

#Print Parameters

Write-Verbose "ResultsFile = $ResultsFile"
Write-Verbose "ThresholdAction = $ThresholdAction"
Write-Verbose "CriticalThreshold = $CriticalThreshold"
Write-Verbose "HighThreshold = $HighThreshold"
Write-Verbose "MediumThreshold = $MediumThreshold"
Write-Verbose "LowThreshold = $LowThreshold"

if ($resultsFile)
{
	Add-Type -assembly "System.IO.Compression.FileSystem"

	try
	{
		$resultsZip = [io.compression.zipfile]::OpenRead("$resultsFile")

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

				$critical = @($jsonContent.runs[0].results | Where-Object { $_.properties.severity -eq "Critical"})
				$high = @($jsonContent.runs[0].results | Where-Object { $_.properties.severity -eq "High"})
				$medium = @($jsonContent.runs[0].results | Where-Object { $_.properties.severity -eq "Medium"})
				$low = @($jsonContent.runs[0].results | Where-Object { $_.properties.severity -eq "Low"})
				$info = @($jsonContent.runs[0].results | Where-Object { $_.properties.severity -eq "Informational"})
				
				Write-Host "Critical: $($critical.length)  Threshold = $CriticalThreshold" -ForegroundColor Green
				Write-Host "High:     $($high.length)  Threshold = $HighThreshold" -ForegroundColor Green
				Write-Host "Medium:   $($medium.length)  Threshold = $MediumThreshold" -ForegroundColor Green
				Write-Host "Low:      $($low.length)  Threshold = $LowThreshold" -ForegroundColor Green
				Write-Host "Info:     $($info.length)" -ForegroundColor Green
				Write-Host "============================================" -ForegroundColor DarkGreen

				$breached = (
							($critical.Length -gt $CriticalThreshold) -or
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
		else
		{
			Write-Error "Couldn't load json from .sarif file"
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

Write-Verbose 'Leaving CheckerQualityGate.ps1'

# End of script