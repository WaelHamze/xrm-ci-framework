[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPing.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\Ping.ps1" -CrmConnectionString $crmConnectionString

Write-Verbose 'Leaving MSCRMPing.ps1'