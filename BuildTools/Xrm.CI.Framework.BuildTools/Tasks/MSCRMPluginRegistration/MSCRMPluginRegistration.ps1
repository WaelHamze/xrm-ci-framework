[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPluginRegistration.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$registrationType = Get-VstsInput -Name registrationType -Require
$assemblyPath = Get-VstsInput -Name assemblyPath -Require
$isWorkflowActivityAssembly = Get-VstsInput -Name isWorkflowActivityAssembly -Require -AsBool
$mappingJsonPath = Get-VstsInput -Name mappingJsonPath
$solutionName = Get-VstsInput -Name solutionName -Require
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "registrationType = $registrationType"
Write-Verbose "assemblyPath = $assemblyPath"
Write-Verbose "isWorkflowActivityAssembly = $isWorkflowActivityAssembly"
Write-Verbose "mappingJsonPath = $mappingJsonPath"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\Lib\xRMCIFramework\9.0.0\PluginRegistration.ps1" -CrmConnectionString $crmConnectionString -RegistrationType $registrationType -AssemblyPath $assemblyPath -IsWorkflowActivityAssembly $isWorkflowActivityAssembly -MappingJsonPath $mappingJsonPath -SolutionName $solutionName -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMPluginRegistration.ps1'