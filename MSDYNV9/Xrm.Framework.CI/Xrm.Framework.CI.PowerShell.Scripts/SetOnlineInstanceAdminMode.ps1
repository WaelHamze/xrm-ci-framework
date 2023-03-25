#
# OnlineInstanceAdminMode.ps1
#

param(
[string]$TenantId , #The tenant Id where your instance resides
[string]$ApplicationId , #The application Id used for the connection
[string]$ApplicationSecret, #The application secret used for connection
[string]$EnvironmentUrl,
[bool]$Enable = $true,
[string]$PowerAppsAdminModulePath,
[string]$CrmConnectorModulePath,
[bool]$AllowBackgroundOperations = $true,
[string]$NotificationText,
[bool]$WaitForCompletion = $false,
[int]$TimeoutInMinutes = 3
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering OnlineInstanceAdminMode.ps1'

#Parameters
Write-Verbose "TenantId = $TenantId"
Write-Verbose "ApplicationId = $ApplicationId"
Write-Verbose "ApplicationSecret = $ApplicationSecret"
Write-Verbose "EnvironmentUrl = $EnvironmentUrl"
Write-Verbose "Enable = $Enable"
Write-Verbose "AllowBackgroundOperations = $AllowBackgroundOperations"
Write-Verbose "NotificationText = $NotificationText"
Write-Verbose "WaitForCompletion = $WaitForCompletion"
Write-Verbose "PowerAppsAdminModulePath = $PowerAppsAdminModulePath"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Import Modules

$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing: $xrmCIToolkit"
Import-Module $xrmCIToolkit

Write-Verbose "Import Crm Connector: $CrmConnectorModulePath"
Import-module "$CrmConnectorModulePath\Microsoft.Xrm.Tooling.CrmConnector.PowerShell.psd1"

Write-Verbose "Importing PowerApps Admin Module: $PowerAppsAdminModulePath"
Import-module "$PowerAppsAdminModulePath\Microsoft.PowerApps.Administration.PowerShell.psd1"

#Connect

Write-Verbose "Connecting to PowerApps Endpoint"
Add-PowerAppsAccount -TenantID $TenantId -ApplicationId $ApplicationId -ClientSecret $ApplicationSecret -Endpoint prod

$CrmConnectionString = "AuthType=ClientSecret;url=$EnvironmentUrl;ClientId=$ApplicationId;ClientSecret=$ApplicationSecret"

$CRMConn = Get-CrmConnection -ConnectionString $CrmConnectionString
$EnvironmentId = $CRMConn.EnvironmentId

if ($Enable)
{
	$RuntimeState = "AdminMode"
}
else
{
	$RuntimeState = "Enabled"
}

Write-Output "Setting Environment $EnvironmentUrl Id:$EnvironmentId to State: $RuntimeState"

$response = Set-AdminPowerAppEnvironmentRuntimeState -EnvironmentName $EnvironmentId -RuntimeState $RuntimeState -WaitUntilFinished $WaitForCompletion -TimeoutInMinutes $TimeoutInMinutes

if ($response.Code -eq 200)
{
	Write-Output "Environment $EnvironmentUrl Id:$EnvironmentId set to State: $RuntimeState"
}
else
{
	throw "Error setting RuntimeState Code:$($response.Code) Error:$($response.Error)"
}

Write-Verbose 'Leaving OnlineInstanceAdminMode.ps1'
