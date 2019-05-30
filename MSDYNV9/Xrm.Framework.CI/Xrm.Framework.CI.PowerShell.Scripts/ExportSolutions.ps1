#
# ExportSolution.ps1
#

param(
[string]$CrmConnectionString,
[string]$SolutionName,
[bool]$ExportManaged,
[bool]$ExportUnmanaged = $true,
[string]$TargetVersion,
[string]$ExportSolutionOutputPath,
[bool]$UpdateVersion,
[string]$RequiredVersion,
[int]$Timeout,
[bool]$ExportIncludeVersionInSolutionName,
[bool]$ExportAutoNumberingSettings,
[bool]$ExportCalendarSettings,
[bool]$ExportCustomizationSettings,
[bool]$ExportEmailTrackingSettings,
[bool]$ExportExternalApplications,
[bool]$ExportGeneralSettings,
[bool]$ExportIsvConfig,
[bool]$ExportMarketingSettings,
[bool]$ExportOutlookSynchronizationSettings,
[bool]$ExportRelationshipRoles,
[bool]$ExportSales
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ExportSolution.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "ExportManaged = $ExportManaged"
Write-Verbose "ExportUnmanaged = $ExportUnmanaged"
Write-Verbose "TargetVersion = $TargetVerion"
Write-Verbose "ExportSolutionOutputPath = $ExportSolutionOutputPath"
Write-Verbose "UpdateVersion = $UpdateVersion"
Write-Verbose "RequiredVersion = $RequiredVersion"
Write-Verbose "Timeout = $Timeout"
Write-Verbose "ExportIncludeVersionInSolutionName = $ExportIncludeVersionInSolutionName"
Write-Verbose "ExportAutoNumberingSettings = $ExportAutoNumberingSettings"
Write-Verbose "ExportCalendarSettings = $ExportCalendarSettings"
Write-Verbose "ExportCustomizationSettings = $ExportCustomizationSettings"
Write-Verbose "ExportEmailTrackingSettings = $ExportEmailTrackingSettings"
Write-Verbose "ExportExternalApplications = $ExportExternalApplications"
Write-Verbose "ExportGeneralSettings = $ExportGeneralSettings"
Write-Verbose "ExportIsvConfig = $ExportIsvConfig"
Write-Verbose "ExportMarketingSettings = $ExportMarketingSettings"
Write-Verbose "ExportOutlookSynchronizationSettings = $ExportOutlookSynchronizationSettings"
Write-Verbose "ExportRelationshipRoles = $ExportRelationshipRoles"
Write-Verbose "ExportSales = $ExportSales"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

#Update Version
if ($UpdateVersion)
{
	Write-Host "Updating Solution Version to $RequiredVersion"
	Set-XrmSolutionVersion -ConnectionString "$CrmConnectionString" -SolutionName $SolutionName -Version $RequiredVersion
	Write-Host "Solution Version Updated"
}

#Solution Export

$exportManagedFile
$exportUnmanagedFile

if ($ExportUnmanaged)
{
    Write-Host "Exporting Unmanaged Solution"
        
    $exportUnmanagedFile = Export-XrmSolution -ConnectionString "$CrmConnectionString" -UniqueSolutionName $SolutionName -OutputFolder "$ExportSolutionOutputPath" -Managed $false -TargetVersion $TargetVersion -IncludeVersionInName $ExportIncludeVersionInSolutionName -ExportAutoNumberingSettings $ExportAutoNumberingSettings -ExportCalendarSettings $ExportCalendarSettings -ExportCustomizationSettings $ExportCustomizationSettings -ExportEmailTrackingSettings $ExportEmailTrackingSettings -ExportExternalApplications $ExportExternalApplications -ExportGeneralSettings $ExportGeneralSettings -ExportMarketingSettings $ExportMarketingSettings -ExportOutlookSynchronizationSettings $ExportOutlookSynchronizationSettings -ExportIsvConfig $ExportIsvConfig -ExportRelationshipRoles $ExportRelationshipRoles -ExportSales $ExportSales -Timeout $Timeout
        
    Write-Host "UnManaged Solution Exported $ExportSolutionOutputPath\$exportUnmanagedFile"
}

if ($ExportManaged)
{
    Write-Host "Exporting Managed Solution"
        
    $exportmanagedFile = Export-XrmSolution -ConnectionString "$CrmConnectionString" -UniqueSolutionName $SolutionName -OutputFolder "$ExportSolutionOutputPath" -Managed $true -TargetVersion $TargetVersion -IncludeVersionInName $ExportIncludeVersionInSolutionName -ExportAutoNumberingSettings $ExportAutoNumberingSettings -ExportCalendarSettings $ExportCalendarSettings -ExportCustomizationSettings $ExportCustomizationSettings -ExportEmailTrackingSettings $ExportEmailTrackingSettings -ExportExternalApplications $ExportExternalApplications -ExportGeneralSettings $ExportGeneralSettings -ExportMarketingSettings $ExportMarketingSettings -ExportOutlookSynchronizationSettings $ExportOutlookSynchronizationSettings -ExportIsvConfig $ExportIsvConfig -ExportRelationshipRoles $ExportRelationshipRoles -ExportSales $ExportSales -Timeout $Timeout
    
    Write-Host "Managed Solution Exported $ExportSolutionOutputPath\$exportmanagedFile"
}

Write-Verbose 'Leaving ExportSolution.ps1'