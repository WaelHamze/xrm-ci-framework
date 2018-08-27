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
$AssemblyName = 'Xrm.CI.Framework.Sample.WFActivities.dll'
$MappingFile = "$scriptPath\PluginRegistration.json"
$Timeout = 360

& "$scriptPath\..\packages\XrmCIFramework.9.0.0.20\tools\GetPluginRegistration.ps1" -Verbose -CrmConnectionString "$CrmConnectionString" -AssemblyName "$AssemblyName" -MappingFile "$MappingFile" -Timeout $Timeout
