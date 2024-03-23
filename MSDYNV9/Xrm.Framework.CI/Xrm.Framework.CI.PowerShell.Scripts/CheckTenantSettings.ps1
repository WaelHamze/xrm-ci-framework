#
# CheckTenantSettings.ps1
#
[CmdletBinding()]

param(
	[string]$TenantId , #The tenant Id where your instance resides
	[string]$ApplicationId , #The application Id used for the connection
	[string]$ApplicationSecret, #The application secret used for connection
	[string]$tenantSettingsFile,
	[string]$PowerAppsAdminModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering CheckTenantSettings.ps1'

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

#Get Tenant Settings

Write-Verbose "Getting Tenant Settings"

$tenantSettings = Get-TenantSettings

$tenantSettingsJson = ConvertTo-Json -InputObject $tenantSettings -Compress -Depth 10

Write-Verbose "Retrieved Tenant Settings: $tenantSettingsJson"

$savedTenantSettings = Get-Content -Path $tenantSettingsFile -Raw

$savedTenantSettingsObject = ConvertFrom-Json $savedTenantSettings

$savedTenantSettingsJson = ConvertTo-Json -InputObject $savedTenantSettingsObject -Compress -Depth 10

Write-Verbose "Saved Tenant Settings: $savedTenantSettingsJson"

if ($tenantSettingsJson -eq $savedTenantSettingsJson)
{
	Write-Host "Tenant Settings Matched"
}
else
{
	throw "Tenant Settings didn't match"
}

Write-Verbose 'Leaving CheckTenantSettings.ps1'
