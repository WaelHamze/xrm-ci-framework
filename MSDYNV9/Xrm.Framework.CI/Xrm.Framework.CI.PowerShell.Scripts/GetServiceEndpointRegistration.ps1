#
# GetPluginRegistration.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$EndpointName,
	[string]$MappingFile,
	[int]$Timeout=360
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering GetServiceEndpointRegistration.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "EndpointName = $EndpointName"
Write-Verbose "MappingFile = $MappingFile"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Get-XrmServiceEndpointRegistration -EndpointName $EndpointName -MappingFile $MappingFile -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Verbose 'Leaving GetServiceEndpointRegistration.ps1' -Verbose