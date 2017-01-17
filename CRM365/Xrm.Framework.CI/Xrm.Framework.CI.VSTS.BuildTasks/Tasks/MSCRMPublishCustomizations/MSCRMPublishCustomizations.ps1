[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPublishCustomizations.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\ps_modules\xRMCIFramework\PublishCustomizations.ps1" -CrmConnectionString $crmConnectionString

Write-Verbose 'Leaving MSCRMPublishCustomizations.ps1'