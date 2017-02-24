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
    [bool]$ExportSales,
    [switch]$OnlyExportIfNewer
)

$ErrorActionPreference = "Stop"

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
Write-Verbose "OnlyExportIfNewer = $OnlyExportIfNewer"
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
    Set-XrmSolutionVersion -ConnectionString $CrmConnectionString -SolutionName $SolutionName -Version $RequiredVersion
    Write-Host "Solution Version Updated"
}

#Solution Export

$exportManagedFile
$exportUnmanagedFile

Function Test-SolutionIsNewer {
    param(
        [string]$ConnectionString,
        [string]$SolutionName,
        [string]$OutputPath,
        [switch]$IsManaged
    )
    
    Write-Verbose "Test-SolutionIsNewer"
    Write-Verbose "ConnectionString = $ConnectionString"
    Write-Verbose "SolutionName = $SolutionName"
    Write-Verbose "OutputPath = $OutputPath"
    Write-Verbose "IsManaged = $IsManaged"

    $sourceSolutionInfo = Get-XrmSolution -ConnectionString $ConnectionString -UniqueSolutionName $SolutionName
    $solutionVersion = $sourceSolutionInfo.Version

    $managed = ""
    if ($IsManaged) {
        $managed = "_managed"
    }

    # First, check for versioned file names, e.g. Solution_1.0.0.0.zip or Solution_1.0.0.0_managed.zip
    $versionedFilename = "$OutputPath\$($SolutionName)_*$managed.zip"

    Write-Verbose "Checking for versioned files in `"$OutputPath`", and taking the highest version if there are multiple files"
    $newestVersionedFilename = Get-ChildItem $versionedFilename `
		| Where-Object { $_.Name -Match "$($SolutionName)_[0-9]+_[0-9]+_[0-9]+_[0-9]+$managed\.zip$" } `
		| Sort-Object { [version](([regex]"([0-9]+_[0-9]+_[0-9]+_[0-9]+)$managed\.zip$").Matches($_).Groups[1].Value -replace "_", ".") } -Descending `
		| Select-Object -ExpandProperty Name -First 1

    if ($newestVersionedFilename -ne $null) {
        Write-Verbose "File with highest version in `"$OutputPath`" is `"$newestVersionedFilename`""

        $currentFileVersion = (([regex]"([0-9]+_[0-9]+_[0-9]+_[0-9]+)$managed\.zip$").Matches($newestVersionedFilename).Groups[1].Value -replace "_", ".")
        Write-Verbose "Version from file is $currentFileVersion"

        if ([Version]$solutionVersion -le [version]$currentFileVersion) {
            Write-Output $true
            Write-Host "Skipped exporting $(if ($IsManaged) { "managed" } else { "unmanaged" }) solution because the source version `"$solutionVersion`" is not higher then the current version `"$currentFileVersion`""
            return
        } else {
            Write-Verbose "New version `"$solutionVersion`" is higher"
        }
    } else {
        Write-Verbose "No versioned file exists in `"$OutputPath`""
    }
    
    # Second, check for unversioned file names, e.g. Solution.zip or Solution_managed.zip
    $unversionedFilename = "$OutputPath\$SolutionName$managed.zip"

    Write-Verbose "Testing if file `"$unversionedFilename`" exists"
    if (Test-Path $unversionedFilename) {
        Write-Verbose "File `"$unversionedFilename`" exists, reading version info from file"
        $currentSolutionInfo = Get-XrmSolutionInfoFromZip -SolutionFilePath $unversionedFilename
        Write-Verbose "Version from file is $($currentSolutionInfo.Version)"
        
        if ([version]$solutionVersion -le [version]$currentSolutionInfo.Version) {
            Write-Output $true
            Write-Host "Skipped exporting $(if ($IsManaged) { "managed" } else { "unmanaged" }) solution because the source version `"$solutionVersion`" is not higher then the current version `"$($currentSolutionInfo.Version)`""
            return
        }

        Write-Verbose "Newer version `"$solutionVersion`" is higher"
    } else {
        Write-Verbose "File `"$unversionedFilename`" doesn't exists"
    }

    Write-Output $false
}

if ($ExportUnmanaged)
{
    $skipExport = $false;
    
    if ($OnlyExportIfNewer) {
        $skipExport = Test-SolutionIsNewer -ConnectionString $CrmConnectionString -SolutionName $SolutionName -OutputPath $ExportSolutionOutputPath
    }

    if (-not $skipExport) {
        Write-Host "Exporting Unmanaged Solution"

        $exportUnmanagedFile = Export-XrmSolution -ConnectionString $CrmConnectionString -UniqueSolutionName $SolutionName -OutputFolder $ExportSolutionOutputPath -Managed $false -TargetVersion $TargetVersion -IncludeVersionInName $ExportIncludeVersionInSolutionName -ExportAutoNumberingSettings $ExportAutoNumberingSettings -ExportCalendarSettings $ExportCalendarSettings -ExportCustomizationSettings $ExportCustomizationSettings -ExportEmailTrackingSettings $ExportEmailTrackingSettings -ExportExternalApplications $ExportExternalApplications -ExportGeneralSettings $ExportGeneralSettings -ExportMarketingSettings $ExportMarketingSettings -ExportOutlookSynchronizationSettings $ExportOutlookSynchronizationSettings -ExportIsvConfig $ExportIsvConfig -ExportRelationshipRoles $ExportRelationshipRoles -ExportSales $ExportSales -Timeout $Timeout

        Write-Host "UnManaged Solution Exported $ExportSolutionOutputPath\$exportUnmanagedFile"
    }
}

if ($ExportManaged)
{
    $skipExport = $false;
    
    if ($OnlyExportIfNewer) {
        $skipExport = Test-SolutionIsNewer -ConnectionString $CrmConnectionString -SolutionName $SolutionName -OutputPath $ExportSolutionOutputPath -IsManaged
    }

    if (-not $skipExport) {
        Write-Host "Exporting Managed Solution"
            
        $exportmanagedFile = Export-XrmSolution -ConnectionString $CrmConnectionString -UniqueSolutionName $SolutionName -OutputFolder $ExportSolutionOutputPath -Managed $true -TargetVersion $TargetVersion -IncludeVersionInName $ExportIncludeVersionInSolutionName -ExportAutoNumberingSettings $ExportAutoNumberingSettings -ExportCalendarSettings $ExportCalendarSettings -ExportCustomizationSettings $ExportCustomizationSettings -ExportEmailTrackingSettings $ExportEmailTrackingSettings -ExportExternalApplications $ExportExternalApplications -ExportGeneralSettings $ExportGeneralSettings -ExportMarketingSettings $ExportMarketingSettings -ExportOutlookSynchronizationSettings $ExportOutlookSynchronizationSettings -ExportIsvConfig $ExportIsvConfig -ExportRelationshipRoles $ExportRelationshipRoles -ExportSales $ExportSales -Timeout $Timeout
        
        Write-Host "Managed Solution Exported $ExportSolutionOutputPath\$exportmanagedFile"
    }
}

Write-Verbose 'Leaving ExportSolution.ps1'