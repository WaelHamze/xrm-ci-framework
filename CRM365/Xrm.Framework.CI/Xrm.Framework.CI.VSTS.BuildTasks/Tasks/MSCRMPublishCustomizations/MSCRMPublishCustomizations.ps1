[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPublishCustomizations.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$connectionTime = Get-VstsInput -Name connectionTime -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "connectionTime = $connectionTime"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\ps_modules\xRMCIFramework\PublishCustomizations.ps1" -CrmConnectionString $crmConnectionString -Timeout $Timeout

Write-Verbose 'Leaving MSCRMPublishCustomizations.ps1'