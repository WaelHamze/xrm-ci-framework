[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPackageDeployer.ps1'

#Get Parameters
$deploymentType= Get-VstsInput -Name deploymentType -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$serverUrl = Get-VstsInput -Name serverUrl
$organizationName = Get-VstsInput -Name organizationName
$deploymentRegion = Get-VstsInput -Name deploymentRegion
$onlineType = Get-VstsInput -Name onlineType
$packageName = Get-VstsInput -Name packageName -Require
$packageDirectory = Get-VstsInput -Name packageDirectory -Require

#TFS Release Parameters
$artifactsFolder = $env:AGENT_RELEASEDIRECTORY 

#Print Verbose
Write-Verbose "deploymentType = $deploymentType"
Write-Verbose "username = $username"
Write-Verbose "password = ******"
Write-Verbose "serverUrl = $serverUrl"
Write-Verbose "organizationName = $organizationName"
Write-Verbose "deploymentRegion = $deploymentRegion"
Write-Verbose "onlineType = $onlineType"
Write-Verbose "packageName = $packageName"
Write-Verbose "packageDirectory = $packageDirectory"
Write-Verbose "artifactsFolder = $artifactsFolder"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\ps_modules\xRMCIFrameworkCI\DeployPackage.ps1" -DeploymentType $deploymentType -Username $username -Password $password -ServerUrl $serverUrl -OrganizationName $organizationName -DeploymentRegion $deploymentRegion -OnlineType $onlineType -PackageName $packageName -PackageDirectory $packageDirectory

Write-Verbose 'Leaving MSCRMPackageDeployer.ps1'
