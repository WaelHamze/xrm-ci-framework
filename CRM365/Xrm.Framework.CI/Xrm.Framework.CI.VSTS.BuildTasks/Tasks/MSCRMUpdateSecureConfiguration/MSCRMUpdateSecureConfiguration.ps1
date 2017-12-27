[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMUpdateSecureConfiguration.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$secureConfiguration = Get-VstsInput -Name secureConfiguration -Require

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "secureConfiguration = $secureConfiguration"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\ps_modules\xRMCIFramework\UpdateSecureConfiguration.ps1" -CrmConnectionString $crmConnectionString -SecureConfiguration $secureConfiguration

Write-Verbose 'Leaving MSCRMUpdateSecureConfiguration.ps1'