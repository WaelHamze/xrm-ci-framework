param(
[string]$CrmConnectionString,
[switch]$PublishCustomizations,
[string]$SolutionImportPackageType,
[switch]$PublishWorkflows,
[switch]$ConvertToManaged,
[switch]$OverwriteUnmanagedCustomizations,
[switch]$SkipProductUpdateDependencies
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "CrmConnectionString: $CrmConnectionString"
Write-Host "PublishCustomizations: $PublishCustomizations"
Write-Host "SolutionImportPackageType: $SolutionImportPackageType"
Write-Host "PublishWorkflows: $PublishWorkflows"
Write-Host "ConvertToManaged: $ConvertToManaged"
Write-Host "OverwriteUnmanagedCustomizations: $OverwriteUnmanagedCustomizations"
Write-Host "SkipProductUpdateDependencies: $SkipProductUpdateDependencies"

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


#Solution Import



#WhoAmI Check
$executingUser = Select-WhoAmI -ConnectionString $CrmConnectionString

Write-Host "Executing User Id" $executingUser.UserId

#Manifest Processing

$manifest = Get-Content ($binariesDirectory + "\manifest.xml")

$outputPath = (Select-Xml -Content $manifest -XPath "//manifest/solution/@path").Node.Value
$managedFile = (Select-Xml -Content $manifest -XPath "//manifest/solution/@managed").Node.Value
$unmanagedFile = (Select-Xml -Content $manifest -XPath "//manifest/solution/@unmanaged").Node.Value

#Solution Import

if ($SolutionImportPackageType -eq "Managed")
{
    $solutionFile = $managedFile
}
elseif ($SolutionImportPackageType -eq "Unmanaged")
{
    $solutionFile = $unmanagedFile   
}
else
{
    throw "$SolutionImportPackageType not supplied"
}

$solutionPath = $binariesDirectory

if ($outputPath)
{
    $solutionPath = $solutionPath + "\" + $outputPath
}

if (-not (Test-Path -Path "$binariesDirectory\logs"))
{
    New-Item "$binariesDirectory\logs" -ItemType directory
}

& "$scriptPath\ImportSolution.ps1" -solutionFile $solutionFile -solutionPath $solutionPath -targetCrmConnectionUrl $CrmConnectionString -override $true -publishWorkflows $PublishWorkflows -overwriteUnmanagedCustomizations $OverwriteUnmanagedCustomizations -skipProductUpdateDependencies $SkipProductUpdateDependencies -logsDirectory "$binariesDirectory\logs"

if ($PublishCustomizations)
{
    Write-Host "Publishing Customizations"

    Publish-XrmCustomizations -ConnectionString $CrmConnectionString

    Write-Host "Publishing Customizations Completed"
}