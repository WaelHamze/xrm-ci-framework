#
# SetTenantSettings.ps1
#
[CmdletBinding()]

param(
	[string]$TenantId , #The tenant Id where your instance resides
	[string]$ApplicationId , #The application Id used for the connection
	[string]$ApplicationSecret, #The application secret used for connection
	[string]$tenantSettingsJson,
	[string]$tenantSettingsJsonFile,
	[string]$PowerAppsAdminModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering SharePowerApps.ps1'

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"


#Import Modules

if ($PowerAppsAdminModulePath)
{
	Write-Verbose "Importing PowerApps Admin Module: $PowerAppsAdminModulePath"
	Import-module "$PowerAppsAdminModulePath\Microsoft.PowerApps.Administration.PowerShell.psd1"
}

#Connect

Write-Verbose "Connecting to PowerApps Endpoint"
If ($applicationId)
{
	Add-PowerAppsAccount -TenantID $TenantId -ApplicationId $ApplicationId -ClientSecret $ApplicationSecret -Endpoint prod
}
else
{
	Add-PowerAppsAccount
}

#Update Tenant Settings

if ($tenantSettingsJsonFile)
{
	$tenantSettingsJson = (Get-Content "$tenantSettingsJsonFile" -Raw)
}

$tenantSettings = ConvertFrom-Json $tenantSettingsJson

Write-Verbose "Updating Tenant Settings to: $tenantSettingsJson"

Set-TenantSettings -RequestBody $tenantSettings

Write-Host "Updated Tenant Settings"

Write-Verbose 'Leaving SetTenantSettings.ps1'
