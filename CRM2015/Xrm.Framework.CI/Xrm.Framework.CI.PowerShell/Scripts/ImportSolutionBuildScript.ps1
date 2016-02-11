# Filename: ImportSolution.ps1

param(
[string]$solutionFile, #The full solution file name
[string]$solutionPathFolder, #The relative folder path containing the solution file
[string]$solutionName, #Unique solution name
[int]$useSolutionName = 0, #Set to 1 if generating file name dynamically using build number
[int]$useBuildNumberForSolutionVerion = 0, #Set to 1 if generating file name dynamically using build number
[string]$CrmConnectionString, #The target CRM organization connection string
[int]$override = 0, #If set to 1 will override the solution even if a solution with same version exists
[int]$publishWorkflows = 0, #Will publish workflows during import
[int]$overwriteUnmanagedCustomizations = 0, #Will overwrite unmanaged customizations
[int]$skipProductUpdateDependencies = 0, #Will skip product update dependencies
[string]$logsDirectory #Optional - will place the import log in here
)

$ErrorActionPreference = "Stop"

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


Write-Output "solutionFile: $solutionFile"
Write-Output "solutionPathFolder: $solutionPathFolder"
Write-Output "CrmConnectionString: $CrmConnectionString"
Write-Output "Override: $override"
Write-Output "Publish Workflows: $publishWorkflows"
Write-Output "overwrite Unmanaged Customizations: $overwriteUnmanagedCustomizations"
Write-Output "Skip Product Update Dependencies: $skipProductUpdateDependencies"
Write-Output "Logs Directory: $logsDirectory"

$solutionPath = $stagingDirectory

if ($solutionPathFolder)
{
    $solutionPath = $solutionPath + "\" + $solutionPathFolder
}

Write-Output "solutionPath: $solutionPath"

if ($useSolutionName -ne 0)
{
    $solutionFile = $solutionName
}
if ($useBuildNumberForSolutionVerion -ne 0)
{
    $VersionNumber = $BuildNumber.Substring($BuildNumber.IndexOf("_") + 1)
    $solutionFile = $solutionFile + "_" + $VersionNumber.replace(".", "_")
}
if ($useSolutionName -ne 0)
{
    $solutionFile = $solutionFile + ".zip"
}

Write-Output "solutionFile: $solutionFile"

& "$scriptPath\ImportSolution.ps1" -solutionFile $solutionFile -solutionPath $solutionPath -CrmConnectionString $CrmConnectionString -override $override -publishWorkflows $publishWorkflows -overwriteUnmanagedCustomizations $overwriteUnmanagedCustomizations -skipProductUpdateDependencies $skipProductUpdateDependencies -logsDirectory $stagingDirectory
