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

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\UpdatePluginAssembly.ps1" -CrmConnectionString $crmConnectionString -AssemblyPath $assemblyPath -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMUpdatePluginAssembly.ps1'