#
# GetPluginRegistrationJsonMapping.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$AssemblyName,
	[string]$MappingJsonPath,
	[int]$Timeout=360
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering GetPluginRegistrationJsonMapping.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "AssemblyName = $AssemblyName"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = "C:\Users\sagarh\source\repos\xrm-ci-framework\MSDYNV9\Xrm.Framework.CI\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Debug\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Get-XrmPluginRegistrationJsonMapping -AssemblyName $AssemblyName -MappingJsonPath $MappingJsonPath -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Verbose 'Leaving GetPluginRegistrationJsonMapping.ps1' -Verbose