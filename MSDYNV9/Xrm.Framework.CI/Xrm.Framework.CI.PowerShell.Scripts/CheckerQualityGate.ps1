#
# Filename: CheckerQualityGate.ps1
#
[CmdletBinding()]

param(
[string]$ResultsFile, #The absolute path to the results file zip to be analyzed
[string]$ThresholdAction, # Warn, Error - The type of action to generate when number of issues exceeds threshold limit
[int]$CriticalThreshold, #Number of critical severity issues
[int]$HighThreshold, #Number of high severity issues
[int]$MediumThreshold, #Number of medium severity issues
[int]$LowThreshold, #Number of low severity issues
[string]$OutputPath #The full path to where you want results to be stored
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
Write-Verbose "OutputPath = $OutputPath"

function HighlightThreshold(
	[int]$count,
	[int]$threshold
	)
	{
		if ($count -gt $threshold)
		{
			return "**$threshold**"
		}
		else
		{
			return "$threshold"
		}
	}

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

				$criticalCount = $critical.Length
				$highCount = $high.Length
				$mediumCount = $medium.Length
				$lowCount = $low.Length
				$infoCount = $info.Length
				
				Write-Host "Critical: $criticalCount  Threshold = $CriticalThreshold" -ForegroundColor Green
				Write-Host "High:     $highCount  Threshold = $HighThreshold" -ForegroundColor Green
				Write-Host "Medium:   $mediumCount  Threshold = $MediumThreshold" -ForegroundColor Green
				Write-Host "Low:      $lowCount  Threshold = $LowThreshold" -ForegroundColor Green
				Write-Host "Info:     $infoCount" -ForegroundColor Green
				Write-Host "============================================" -ForegroundColor DarkGreen

				$md = "| Severity | Count | Threshold |  `r`n"
				$md += "|:-----------|:-----------:|:-----------:|  `r`n"
				$md += "| Critical | $criticalCount | $(HighlightThreshold $criticalCount $CriticalThreshold) |  `r`n"
				$md += "| High | $highCount | $(HighlightThreshold $highCount $HighThreshold) |  `r`n"
				$md += "| Medium | $mediumCount | $(HighlightThreshold $mediumCount $MediumThreshold) |  `r`n"
				$md += "| Low | $lowCount | $(HighlightThreshold $lowCount $LowThreshold) |  `r`n"
				$md += "| Informational | $infoCount | N/A |  `r`n"

				Set-Content -Path "$OutputPath\CheckerThresholdsSummary.md" -Value "$md"

				$breached = (
							($criticalCount -gt $CriticalThreshold) -or
							($highCount -gt $HighThreshold) -or
							($mediumCount -gt $MediumThreshold) -or
							($lowCount -gt $LowThreshold)
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