param(
[string]$AuthenticationType,
[string]$Username,
[string]$Password,
[string]$ServerUrl,
[string]$OrganizationName,
[string]$DeploymentRegion,
[string]$OnlineType,
[string]$HomRealmURL,
[string]$PackageName
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "AuthenticationType: $AuthenticationType"
Write-Host "Username: $Username"
Write-Host "Password: $Password"
Write-Host "ServerUrl: $ServerUrl"
Write-Host "OrganizationName: $OrganizationName"
Write-Host "DeploymentRegion: $DeploymentRegion"
Write-Host "OnlineType: $OnlineType"
Write-Host "HomRealmURL: $HomRealmURL"
Write-Host "PackageName: $PackageName"

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

#Deploy Package

& "$scriptPath\DeployPackage.ps1" -AuthenticationType $AuthenticationType -Username $Username -Password $Password -ServerUrl $ServerUrl -OrganizationName $OrganizationName -DeploymentRegion $DeploymentRegion -OnlineType $OnlineType -HomRealmURL $HomRealmURL -PackageName $PackageName -PackageDirectory $binariesDirectory