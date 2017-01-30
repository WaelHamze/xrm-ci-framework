[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMExportSolution.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$solutionName = Get-VstsInput -Name solutionName -Require
$exportManaged = Get-VstsInput -Name exportManaged -Require -AsBool
$exportUnmanaged = Get-VstsInput -Name exportUnmanaged -Require -AsBool
$includeVersionInSolutionFile = Get-VstsInput -Name includeVersionInSolutionFile -AsBool
$targetVersion = Get-VstsInput -Name targetVersion
$updateVersion = Get-VstsInput -Name updateVersion -AsBool
$includeVersionInSolutionFile = Get-VstsInput -Name includeVersionInSolutionFile -AsBool
$outputPath = Get-VstsInput -Name outputPath -Require
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt
$exportAutoNumberingSettings = Get-VstsInput -Name exportAutoNumberingSettings -AsBool
$exportCalendarSettings = Get-VstsInput -Name exportCalendarSettings -AsBool
$exportCustomizationSettings = Get-VstsInput -Name exportCustomizationSettings -AsBool
$ExportEmailTrackingSettings = Get-VstsInput -Name exportEmailTrackingSettings -AsBool
$exportExternalApplications = Get-VstsInput -Name exportExternalApplications -AsBool
$exportGeneralSettings = Get-VstsInput -Name exportGeneralSettings -AsBool
$exportIsvConfig = Get-VstsInput -Name exportIsvConfig -AsBool
$exportMarketingSettings = Get-VstsInput -Name exportMarketingSettings -AsBool
$exportOutlookSynchronizationSettings = Get-VstsInput -Name exportOutlookSynchronizationSettings -AsBool
$exportRelationshipRoles = Get-VstsInput -Name exportRelationshipRoles -AsBool
$exportSales = Get-VstsInput -Name exportSales -AsBool

#TFS Build Parameters
$buildNumber = $env:BUILD_BUILDNUMBER
$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY
$binariesDirectory = $env:BUILD_BINARIESDIRECTORY

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "exportManaged = $exportManaged"
Write-Verbose "exportUnmanaged = $exportUnmanaged"
Write-Verbose "includeVersionInSolutionFile = $includeVersionInSolutionFile"
Write-Verbose "targetVersion = $targetVersion"
Write-Verbose "updateVersion = $updateVersion"
Write-Verbose "includeVersionInSolutionFile = $includeVersionInSolutionFile"
Write-Verbose "outputPath = $outputPath"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"
Write-Verbose "exportAutoNumberingSettings = $exportAutoNumberingSettings"
Write-Verbose "exportCalendarSettings = $exportCalendarSettings"
Write-Verbose "exportCustomizationSettings = $exportCustomizationSettings"
Write-Verbose "ExportEmailTrackingSettings = $ExportEmailTrackingSettings"
Write-Verbose "exportExternalApplications = $exportExternalApplications"
Write-Verbose "exportGeneralSettings = $exportGeneralSettings"
Write-Verbose "exportIsvConfig = $exportIsvConfig"
Write-Verbose "exportMarketingSettings = $exportMarketingSettings"
Write-Verbose "exportOutlookSynchronizationSettings = $exportOutlookSynchronizationSettings"
Write-Verbose "exportRelationshipRoles = $exportRelationshipRoles"
Write-Verbose "exportSales = $exportSales"

Write-Verbose "buildNumber = $buildNumber"
Write-Verbose "sourcesDirectory = $sourcesDirectory"
Write-Verbose "binariesDirectory = $binariesDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

if ($updateVersion)
{
	$versionNumber = $buildNumber.Substring($buildNumber.IndexOf("_") + 1)
}

& "$scriptPath\ps_modules\xRMCIFramework\ExportSolution.ps1"  -CrmConnectionString $crmConnectionString -SolutionName $solutionName -ExportManaged $exportManaged -ExportUnmanaged $exportUnmanaged -ExportSolutionOutputPath $outputPath -TargetVersion $targetVersion -UpdateVersion $updateVersion -RequiredVersion $versionNumber -ExportIncludeVersionInSolutionName $includeVersionInSolutionFile -ExportAutoNumberingSettings $exportAutoNumberingSettings -ExportCalendarSettings $exportCalendarSettings -ExportCustomizationSettings $exportCustomizationSettings -ExportEmailTrackingSettings $exportEmailTrackingSettings -ExportExternalApplications $exportExternalApplications -ExportGeneralSettings $exportGeneralSettings -ExportMarketingSettings $exportMarketingSettings -ExportOutlookSynchronizationSettings $exportOutlookSynchronizationSettings -ExportIsvConfig $exportIsvConfig -ExportRelationshipRoles $exportRelationshipRoles -ExportSales $exportSales -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMExportSolution.ps1'
