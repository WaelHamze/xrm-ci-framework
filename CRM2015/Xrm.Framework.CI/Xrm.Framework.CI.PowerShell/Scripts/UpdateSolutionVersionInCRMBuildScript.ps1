param(
[string]$CrmConnectionString,
[string]$SolutionName
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "CrmConnectionString: $CrmConnectionString"
Write-Host "SolutionName: $SolutionName"
Write-Host "VersionNumber: $VersionNumber"

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

$VersionNumber = $BuildNumber.Substring($BuildNumber.IndexOf("_") + 1)

Write-Host "VersionNumber extracted from Build: $VersionNumber"

& "$scriptPath\UpdateSolutionVersionInCRM.ps1" -CrmConnectionString $CrmConnectionString -SolutionName $SolutionName -VersionNumber $VersionNumber
