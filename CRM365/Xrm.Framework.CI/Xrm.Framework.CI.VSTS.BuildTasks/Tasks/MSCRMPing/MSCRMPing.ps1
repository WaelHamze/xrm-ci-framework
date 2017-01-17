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

& "$scriptPath\ps_modules\xRMCIFramework\Ping.ps1" -CrmConnectionString $crmConnectionString

Write-Verbose 'Leaving MSCRMPing.ps1'