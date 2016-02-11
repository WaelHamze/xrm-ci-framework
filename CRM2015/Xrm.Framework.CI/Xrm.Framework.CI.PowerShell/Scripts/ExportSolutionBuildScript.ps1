param(
[string]$CrmConnectionString,
[string]$SolutionName,
[int]$ExportManaged = 0,
[int]$ExportUnmanaged = 1,
[string]$ExportSolutionOutputPath,
[int]$ExportIncludeVersionInSolutionName = 1,
[int]$ExportAutoNumberingSettings = 0,
[int]$ExportCalendarSettings = 0,
[int]$ExportCustomizationSettings = 0,
[int]$ExportEmailTrackingSettings = 0,
[int]$ExportGeneralSettings = 0,
[int]$ExportIsvConfig = 0,
[int]$ExportMarketingSettings = 0,
[int]$ExportOutlookSynchronizationSettings = 0,
[int]$ExportRelationshipRoles = 0
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "CrmConnectionString: $CrmConnectionString"
Write-Host "SolutionName: $SolutionName"
Write-Host "ExportManaged: $ExportManaged"
Write-Host "ExportUnmanaged: $ExportUnmanaged"
Write-Host "ExportIncludeVersionInSolutionName: $ExportIncludeVersionInSolutionName"
Write-Host "ExportAutoNumberingSettings: $ExportAutoNumberingSettings"
Write-Host "ExportCalendarSettings: $ExportCalendarSettings"
Write-Host "ExportCustomizationSettings: $ExportCustomizationSettings"
Write-Host "ExportEmailTrackingSettings: $ExportEmailTrackingSettings"
Write-Host "ExportGeneralSettings: $ExportGeneralSettings"
Write-Host "ExportIsvConfig: $ExportIsvConfig"
Write-Host "ExportMarketingSettings: $ExportMarketingSettings"
Write-Host "ExportOutlookSynchronizationSettings: $ExportOutlookSynchronizationSettings"
Write-Host "ExportRelationshipRoles: $ExportRelationshipRoles"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

#TFS Build Parameters
$buildNumber = $env:BUILD_BUILDNUMBER
$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY
$stagingDirectory = $env:BUILD_STAGINGDIRECTORY 

Write-Host "buildNumber: $buildNumber"
Write-Host "sourcesDirectory: $sourcesDirectory"
Write-Host "stagingDirectory: $stagingDirectory"


& "$scriptPath\ExportSolution.ps1" -CrmConnectionString "$CrmConnectionString" -ExportSolutionOutputPath "$stagingDirectory" -SolutionName "$SolutionName"