#
# PluginRegistration.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$AssemblyPath,
	[string]$MappingJsonPath,
	[string]$SolutionName,
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering PluginRegistration.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "AssemblyPath = $AssemblyPath"
Write-Verbose "MappingJsonPath = $MappingJsonPath"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Host "Updating Plugin Assembly: $AssemblyPath"

Set-XrmPluginRegistration -AssemblyPath $AssemblyPath -MappingJsonPath $MappingJsonPath -SolutionName $SolutionName -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Host "Updated Plugin Assembly and Steps"

Write-Verbose 'Leaving PluginRegistration.ps1' -Verbose