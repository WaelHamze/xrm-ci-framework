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
Write-Host "ExportSolutionOutputPath: $ExportSolutionOutputPath"
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

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.dll"
Write-Host "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Host "Imported CIToolkit"


#Solution Export

$exportManagedFile
$exportUnmanagedFile

#WhoAmI Check
$executingUser = Select-WhoAmI -ConnectionString $CrmConnectionString

Write-Host "Executing User Id" $executingUser.UserId

if ($ExportUnmanaged -ne 0)
{
    Write-Host "Exporting Unmanaged Solution"
        
    $exportUnmanagedFile = Export-XrmSolution -ConnectionString $CrmConnectionString -UniqueSolutionName $SolutionName -OutputFolder $ExportSolutionOutputPath -Managed $false -IncludeVersionInName $ExportIncludeVersionInSolutionName -ExportAutoNumberingSettings $ExportAutoNumberingSettings -ExportCalendarSettings $ExportCalendarSettings -ExportCustomizationSettings $ExportCustomizationSettings -ExportEmailTrackingSettings $ExportEmailTrackingSettings -ExportGeneralSettings $ExportGeneralSettings -ExportMarketingSettings $ExportMarketingSettings -ExportOutlookSynchronizationSettings $ExportOutlookSynchronizationSettings -ExportIsvConfig $ExportIsvConfig -ExportRelationshipRoles $ExportRelationshipRoles
        
    Write-Host "UnManaged Solution Exported $ExportSolutionOutputPath\$exportUnmanagedFile"
}

if ($ExportManaged -ne 0)
{
    Write-Host "Exporting Managed Solution"
        
    $exportmanagedFile = Export-XrmSolution -ConnectionString $CrmConnectionString -UniqueSolutionName $SolutionName -OutputFolder $ExportSolutionOutputPath -Managed $true -IncludeVersionInName $ExportIncludeVersionInSolutionName -ExportAutoNumberingSettings $ExportAutoNumberingSettings -ExportCalendarSettings $ExportCalendarSettings -ExportCustomizationSettings $ExportCustomizationSettings -ExportEmailTrackingSettings $ExportEmailTrackingSettings -ExportGeneralSettings $ExportGeneralSettings -ExportMarketingSettings $ExportMarketingSettings -ExportOutlookSynchronizationSettings $ExportOutlookSynchronizationSettings -ExportIsvConfig $ExportIsvConfig -ExportRelationshipRoles $ExportRelationshipRoles
    
    Write-Host "Managed Solution Exported $ExportSolutionOutputPath\$exportmanagedFile"
}
