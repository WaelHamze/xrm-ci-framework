[CmdletBinding()]

param
(
	[string]$CrmConnectionString #The connection string as per CRM Sdk
)

$ErrorActionPreference = "Stop"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

Write-Verbose "ConnectionString = $connectionString"

if ($CrmConnectionString -eq '')
{
	$CrmConnectionString = $Env:CrmCon
}
$AssemblyPath = "$scriptPath\bin\Debug\Xrm.CI.Framework.Sample.WFActivities.dll"
$SolutionName = 'xRMCISample'
$MappingJsonPath = "$scriptPath\PluginRegistration.json"
$IsWorkflowActivityAssembly = $true
$RegistrationType = "upsert"

& "$scriptPath\..\packages\XrmCIFramework.9.0.0.17\tools\PluginRegistration.ps1" -Verbose -CrmConnectionString "$CrmConnectionString" -AssemblyPath "$AssemblyPath" -MappingJsonPath "$MappingJsonPath" -SolutionName $SolutionName -IsWorkflowActivityAssembly $IsWorkflowActivityAssembly -RegistrationType $RegistrationType