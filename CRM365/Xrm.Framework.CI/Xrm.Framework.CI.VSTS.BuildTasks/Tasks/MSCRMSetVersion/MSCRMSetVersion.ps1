[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMSetVersion.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$solutionName = Get-VstsInput -Name solutionName -Require
$versionNumber = Get-VstsInput -Name versionNumber -Require

#TFS Build Parameters
$buildNumber = $env:BUILD_BUILDNUMBER
$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY
$binariesDirectory = $env:BUILD_BINARIESDIRECTORY

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "versionNumber = $versionNumber"

Write-Verbose "buildNumber = $buildNumber"
Write-Verbose "sourcesDirectory = $sourcesDirectory"
Write-Verbose "binariesDirectory = $binariesDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\ps_modules\xRMCIFramework\UpdateSolutionVersionInCRM.ps1" -CrmConnectionString $crmConnectionString -SolutionName $solutionName -VersionNumber $versionNumber

Write-Verbose 'Leaving MSCRMSetVersion.ps1'
