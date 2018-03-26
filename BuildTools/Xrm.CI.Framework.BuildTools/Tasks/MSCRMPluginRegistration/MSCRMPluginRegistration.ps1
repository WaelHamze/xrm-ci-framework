[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPluginRegistration.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$assemblyPath = Get-VstsInput -Name assemblyPath -Require
$mappingJsonPath = Get-VstsInput -Name mappingJsonPath -Require
$solutionName = Get-VstsInput -Name solutionName -Require
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "assemblyPath = $assemblyPath"
Write-Verbose "mappingJsonPath = $mappingJsonPath"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\Lib\xRMCIFramework\9.0.0\PluginRegistration.ps1" -CrmConnectionString $crmConnectionString -AssemblyPath $assemblyPath -MappingJsonPath $mappingJsonPath -SolutionName $solutionName -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMPluginRegistration.ps1'