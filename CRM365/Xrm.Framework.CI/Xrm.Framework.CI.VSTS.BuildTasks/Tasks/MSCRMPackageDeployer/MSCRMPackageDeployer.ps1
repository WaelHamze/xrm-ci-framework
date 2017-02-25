[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPackageDeployer.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$packageName = Get-VstsInput -Name packageName -Require
$packageDirectory = Get-VstsInput -Name packageDirectory -Require

#TFS Release Parameters
$artifactsFolder = $env:AGENT_RELEASEDIRECTORY 

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "packageName = $packageName"
Write-Verbose "packageDirectory = $packageDirectory"
Write-Verbose "artifactsFolder = $artifactsFolder"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\ps_modules\xRMCIFramework\DeployPackage.ps1" -CrmConnectionString $crmConnectionString -PackageName $packageName -PackageDirectory $packageDirectory

Write-Verbose 'Leaving MSCRMPackageDeployer.ps1'
