param(
[switch]$EnableSolutionExport,
[string]$CrmConnectionString,
[switch]$SolutionUpdateSetVersionToBuild,
[switch]$PublishCustomizations,
[string]$SolutionName,
[string]$SolutionExportPackageType,
[string]$ExportSolutionOutputPath,
[switch]$ExportIncludeVersionInSolutionName,
[switch]$ExportAutoNumberingSettings,
[switch]$ExportCalendarSettings,
[switch]$ExportCustomizationSettings,
[switch]$ExportEmailTrackingSettings,
[switch]$ExportGeneralSettings,
[switch]$ExportIsvConfig,
[switch]$ExportMarketingSettings,
[switch]$ExportOutlookSynchronizationSettings,
[switch]$ExportRelationshipRoles,
[switch]$EnableSolutionPack,
[switch]$SolutionPackSetVersionToBuild,
[switch]$CheckInUpdatedSolutionFile,
[string]$LocalSolutionPackagerFile,
[string]$LocalSolutionPackagerFilesFolder,
[string]$LocalSolutionPackagerMappingFile,
[string]$SolutionPackPackageType,
[string]$SolutionPackOutputPath,
[switch]$PackIncludeVersionInSolutionName,
[string]$TfCommand
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "EnableSolutionExport: $EnableSolutionExport"
Write-Host "CrmConnectionString: $CrmConnectionString"
Write-Host "SolutionUpdateSetVersionToBuild: $SolutionUpdateSetVersionToBuild"
Write-Host "PublishCustomizations: $PublishCustomizations"
Write-Host "SolutionName: $SolutionName"
Write-Host "SolutionExportPackageType: $SolutionExportPackageType"
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
Write-Host "EnableSolutionPack: $EnableSolutionPack"
Write-Host "SolutionPackSetVersionToBuild: $SolutionPackSetVersionToBuild"
Write-Host "CheckInUpdatedSolutionFile: $CheckInUpdatedSolutionFile"
Write-Host "LocalSolutionPackagerFile: $LocalSolutionPackagerFile"
Write-Host "LocalSolutionPackagerFilesFolder: $LocalSolutionPackagerFilesFolder"
Write-Host "LocalSolutionPackagerMappingFile: $LocalSolutionPackagerMappingFile"
Write-Host "SolutionPackPackageType: $SolutionPackPackageType"
Write-Host "SolutionPackOutputPath: $SolutionPackOutputPath"
Write-Host "PackIncludeVersionInSolutionName: $PackIncludeVersionInSolutionName"
Write-Host "TfCommand: $TfCommand"

#TFS Build Parameters
$buildNumber = $env:TF_BUILD_BUILDNUMBER
$sourcesDirectory = $env:TF_BUILD_SOURCESDIRECTORY
$binariesDirectory = $env:TF_BUILD_BINARIESDIRECTORY

Write-Host "buildNumber: $buildNumber"
Write-Host "sourcesDirectory: $sourcesDirectory"
Write-Host "binariesDirectory: $binariesDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.dll"
Write-Host "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Host "Imported CIToolkit"

#Functions
function ConvertToBool($switchValue)
{
    if ($switchValue)
    {
        return $true
    }
    else
    {
        return $false
    }
}


#Solution Export

$exportManagedFile
$exportUnmanagedFile

if ($EnableSolutionExport)
{
    #WhoAmI Check
    $executingUser = Select-WhoAmI -ConnectionString $CrmConnectionString

    Write-Host "Executing User Id" $executingUser.UserId

    if ($PublishCustomizations)
    {
        Write-Host "Publishing Customizations"

        Publish-XrmCustomizations -ConnectionString $CrmConnectionString

        Write-Host "Publishing Customizations Completed"
    }

    if ($SolutionUpdateSetVersionToBuild)
    {
        $versionNumber = $buildNumber.Substring($buildNumber.IndexOf("_") + 1)

        Write-Host "Updating Solution Version to $versionNumber"

        Set-XrmSolutionVersion -ConnectionString $CrmConnectionString -SolutionName $SolutionName -Version $versionNumber

        Write-Host "Solution Version Updated"
    }

    $exportPath = $binariesDirectory

    if ($ExportSolutionOutputPath)
    {
        $exportPath = $binariesDirectory + "\" + $ExportSolutionOutputPath 
    }

    if ($SolutionExportPackageType -ne "Managed")
    {
        Write-Host "Exporting Unmanaged Solution"
        
        $exportUnmanagedFile = Export-XrmSolution -ConnectionString $CrmConnectionString -UniqueSolutionName $SolutionName -OutputFolder $exportPath -Managed $false -IncludeVersionInName (ConvertToBool $ExportIncludeVersionInSolutionName) -ExportAutoNumberingSettings (ConvertToBool $ExportAutoNumberingSettings) -ExportCalendarSettings (ConvertToBool $ExportCalendarSettings) -ExportCustomizationSettings (ConvertToBool $ExportCustomizationSettings) -ExportEmailTrackingSettings (ConvertToBool $ExportEmailTrackingSettings) -ExportGeneralSettings (ConvertToBool $ExportGeneralSettings) -ExportMarketingSettings (ConvertToBool $ExportMarketingSettings) -ExportOutlookSynchronizationSettings (ConvertToBool $ExportOutlookSynchronizationSettings) -ExportIsvConfig (ConvertToBool $ExportIsvConfig) -ExportRelationshipRoles (ConvertToBool $ExportRelationshipRoles)
        
        Write-Host "UnManaged Solution Exported $exportPath\$exportUnmanagedFile"
    }

    if ($SolutionExportPackageType -ne "Unmanaged")
    {
        Write-Host "Exporting Managed Solution"
        
        $exportmanagedFile = Export-XrmSolution -ConnectionString $CrmConnectionString -UniqueSolutionName $SolutionName -OutputFolder $exportPath -Managed $true -IncludeVersionInName (ConvertToBool $ExportIncludeVersionInSolutionName) -ExportAutoNumberingSettings (ConvertToBool $ExportAutoNumberingSettings) -ExportCalendarSettings (ConvertToBool $ExportCalendarSettings) -ExportCustomizationSettings (ConvertToBool $ExportCustomizationSettings) -ExportEmailTrackingSettings (ConvertToBool $ExportEmailTrackingSettings) -ExportGeneralSettings (ConvertToBool $ExportGeneralSettings) -ExportMarketingSettings (ConvertToBool $ExportMarketingSettings) -ExportOutlookSynchronizationSettings (ConvertToBool $ExportOutlookSynchronizationSettings) -ExportIsvConfig (ConvertToBool $ExportIsvConfig) -ExportRelationshipRoles (ConvertToBool $ExportRelationshipRoles)
    
        Write-Host "Managed Solution Exported $exportPath\$exportmanagedFile"
    }

    $exportManifest = '<manifest><solution name="' + $SolutionName + '" path="' + $ExportSolutionOutputPath + '" managed="' + $exportmanagedFile + '" unmanaged="' + $exportUnmanagedFile + '"></solution></manifest>'

    $exportManifest | Out-File ($binariesDirectory + "\manifest.xml")
}


#Solution Pack

$packManagedFile
$packUnmanagedFile

if ($EnableSolutionPack)
{

    if ($SolutionPackSetVersionToBuild)
    {
        $versionNumber = $buildNumber.Substring($buildNumber.IndexOf("_") + 1)
        
        Write-Host "Setting Solution Version in File to: $versionNumber"
        
        if ($CheckInUpdatedSolutionFile)
        {
            & "$tfCommand" checkout /noprompt "$LocalSolutionPackagerFilesFolder\Other\Solution.xml"

            $lastexitcode
        }
        else
        {
            Set-ItemProperty "$LocalSolutionPackagerFilesFolder\Other\Solution.xml" -name IsReadOnly -value $false
        }

        Set-XrmSolutionVersionInFolder -SolutionFilesFolderPath $LocalSolutionPackagerFilesFolder -Version $versionNumber

        if($CheckInUpdatedSolutionFile)
        {
            & "$tfCommand" checkin "$LocalSolutionPackagerFilesFolder\Other\Solution.xml" /noprompt /comment:"Update Solution Version" /override:"CI Build" /bypass /force

            $lastexitcode
        }

        Write-Host "Version Updated & Checked-In"
    }

    $solutionInfo = Get-XrmSolutionInfoFromFolder -SolutionFilesFolderPath $LocalSolutionPackagerFilesFolder
    $packSolutionName = $solutionInfo.UniqueName
    $packSolutionVersion = $solutionInfo.Version
    
    Write-Host "Packing Solution $packSolutionName, Version $packSolutionVersion"

    $packStringBuilder = $packSolutionName
    if ($PackIncludeVersionInSolutionName)
    {
        $packStringBuilder = $packStringBuilder + "_" + $packSolutionVersion.replace(".", "_")
    }
    $packManagedFile = $packStringBuilder + "_managed.zip"
    $packUnmanagedFile = $packStringBuilder + ".zip"

    $packPath = $binariesDirectory
    if ($SolutionPackOutputPath)
    {
        $packPath = $packPath + "\" + $SolutionPackOutputPath
    }

    $targetFile = $packPath + "\" + $packUnmanagedFile

    if ($LocalSolutionPackagerMappingFile)
    {
        $packOutput = & "$LocalSolutionPackagerFile" /action:Pack /zipfile:"$targetFile" /folder:"$LocalSolutionPackagerFilesFolder" /packagetype:$SolutionPackPackageType /map:"$LocalSolutionPackagerMappingFile"
    }
    else
    {
        $packOutput = & "$LocalSolutionPackagerFile" /action:Pack /zipfile:"$targetFile" /folder:"$LocalSolutionPackagerFilesFolder" /packagetype:$SolutionPackPackageType
    }

	Write-Output $packOutput
	if ($lastexitcode -ne 0)
	{
		throw "Solution Pack operation failed with exit code: $lastexitcode"
	}
	else
	{
		if (($packOutput -ne $null) -and ($packOutput -like "*warnings encountered*"))
		{
			Write-Warning "Solution Packager encountered warnings. Check the output."
		}
	}

    $packManifest = '<manifest><solution name="' + $packSolutionName + '" path="' + $SolutionPackOutputPath + '" managed="' + $packManagedFile + '" unmanaged="' + $packUnmanagedFile + '"></solution></manifest>'

    $packManifest | Out-File ($binariesDirectory + "\manifest.xml")
}
