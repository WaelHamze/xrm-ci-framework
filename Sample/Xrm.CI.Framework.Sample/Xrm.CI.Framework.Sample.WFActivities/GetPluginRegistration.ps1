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
$AssemblyName = 'Xrm.CI.Framework.Sample.WFActivities'
$MappingJsonPath = "$scriptPath\PluginRegistrationMapping.json"
$IsWorkflowActivityAssembly = $true
$SolutionName = 'xRMCISample'

& "$scriptPath\..\packages\XrmCIFramework.9.0.0.17\tools\GetPluginRegistrationJsonMapping.ps1" -Verbose -CrmConnectionString "$CrmConnectionString" -AssemblyName "$AssemblyName" -MappingJsonPath "$MappingJsonPath" -IsWorkflowActivityAssembly $IsWorkflowActivityAssembly -SolutionName $SolutionName
