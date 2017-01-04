#
# ExportSolution.ps1
#

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

Write-Verbose 'Entering ExportSolution.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "ExportManaged = $ExportManaged"
Write-Verbose "ExportUnmanaged = $ExportUnmanaged"
Write-Verbose "ExportSolutionOutputPath = $ExportSolutionOutputPath"
Write-Verbose "ExportIncludeVersionInSolutionName = $ExportIncludeVersionInSolutionName"
Write-Verbose "ExportAutoNumberingSettings = $ExportAutoNumberingSettings"
Write-Verbose "ExportCalendarSettings = $ExportCalendarSettings"
Write-Verbose "ExportCustomizationSettings = $ExportCustomizationSettings"
Write-Verbose "ExportEmailTrackingSettings = $ExportEmailTrackingSettings"
Write-Verbose "ExportGeneralSettings = $ExportGeneralSettings"
Write-Verbose "ExportIsvConfig = $ExportIsvConfig"
Write-Verbose "ExportMarketingSettings = $ExportMarketingSettings"
Write-Verbose "ExportOutlookSynchronizationSettings = $ExportOutlookSynchronizationSettings"
Write-Verbose "ExportRelationshipRoles = $ExportRelationshipRoles"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"


#Solution Export

$exportManagedFile
$exportUnmanagedFile

#WhoAmI Check
$executingUser = Select-WhoAmI -ConnectionString $CrmConnectionString

Write-Verbose "Executing User Id" $executingUser.UserId

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

Write-Verbose 'Leaving ExportSolution.ps1'