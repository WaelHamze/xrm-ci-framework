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

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\UpdateSecureConfiguration.ps1" -CrmConnectionString $crmConnectionString -SecureConfiguration $secureConfiguration

Write-Verbose 'Leaving MSCRMUpdateSecureConfiguration.ps1'