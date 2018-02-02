[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMUpdatePluginAssembly.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$assemblyPath = Get-VstsInput -Name assemblyPath -Require
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "assemblyPath = $assemblyPath"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\Lib\xRMCIFramework\9.0.0\UpdatePluginAssembly.ps1" -CrmConnectionString $crmConnectionString -AssemblyPath $assemblyPath -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMUpdatePluginAssembly.ps1'