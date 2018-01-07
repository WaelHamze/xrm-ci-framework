[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMUpdateWebResource.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$webResourceProjectPath = Get-VstsInput -Name webResourceProjectPath -Require
$publish = Get-VstsInput -Name publish
$timeout = Get-VstsInput -Name timeout -Require

#Print Verbose
Write-Verbose "CrmConnectionString = $crmConnectionString"
Write-Verbose "WebResourceProjectPath = $webResourceProjectPath"
Write-Verbose "Publish = $publish"
Write-Verbose "Timeout = $timeout"
#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\Lib\xRMCIFramework\9.0.0\UpdateWebResource.ps1" -CrmConnectionString $crmConnectionString -WebResourceProjectPath $webResourceProjectPath -Publish $publish -Timeout $timeout

Write-Verbose 'Leaving MSCRMUpdateWebResource.ps1'