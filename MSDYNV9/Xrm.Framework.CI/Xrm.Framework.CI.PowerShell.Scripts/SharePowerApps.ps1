#
# SharePowerApps.ps1
#
[CmdletBinding()]

param(
	[string]$TenantId , #The tenant Id where your instance resides
	[string]$ApplicationId , #The application Id used for the connection
	[string]$ApplicationSecret, #The application secret used for connection
	[string]$EnvironmentUrl,
	[string]$AppsToShareJson,
	[string]$MSALModulePath,
	[string]$MGUsersModulePath,
	[string]$MGGroupsModulePath,
	[string]$PowerAppsModulePath,
	[string]$PowerAppsAdminModulePath,
    [string]$CrmConnectorModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering SharePowerApps.ps1'

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Import Modules

$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing: $xrmCIToolkit"
Import-Module $xrmCIToolkit

Write-Verbose "Import Crm Connector: $CrmConnectorModulePath"
Import-module "$CrmConnectorModulePath\Microsoft.Xrm.Tooling.CrmConnector.PowerShell.psd1"

Write-Verbose "Importing MSAL Module" 
Import-Module "$MSALModulePath\MSAL.PS.psd1"

Write-Verbose "Importing Microsoft Graph Users Module" 
Import-Module "$MGUsersModulePath\Microsoft.Graph.Users.psd1"

Write-Verbose "Importing Microsoft Graph Groups Module" 
Import-Module "$MGGroupsModulePath\Microsoft.Graph.Groups.psd1"

Write-Verbose "Importing PowerApps Admin Module: $PowerAppsAdminModulePath"
Import-module "$PowerAppsAdminModulePath\Microsoft.PowerApps.Administration.PowerShell.psd1"

#Connect

Write-Verbose "Connecting to Microsoft Graph"
$MsalToken = Get-MsalToken -TenantId $TenantId -ClientId $ApplicationId -ClientSecret ($ApplicationSecret | ConvertTo-SecureString -AsPlainText -Force)
Connect-Graph -AccessToken $MsalToken.AccessToken

Write-Verbose "Connecting to PowerApps Endpoint"
Add-PowerAppsAccount -TenantID $TenantId -ApplicationId $ApplicationId -ClientSecret $ApplicationSecret -Endpoint prod

#Environment

#$EnvironmentName = (Get-AdminPowerAppEnvironment "$EnvironmentDisplayName").EnvironmentName
#if ($EnvironmentName)
#{
#	Write-Verbose "Environment found with Id: $EnvironmentName"
#}
#else
#{
#	throw "Evironment: $EnvironmentDisplayName could not be found"
#}


$CrmConnectionString = "AuthType=ClientSecret;url=$EnvironmentUrl;ClientId=$ApplicationId;ClientSecret=$ApplicationSecret"


$CRMConn = Get-CrmConnection -ConnectionString $CrmConnectionString
$EnvironmentId = $CRMConn.EnvironmentId

#Azure AD

#$securePassword = ConvertTo-SecureString $ApplicationSecret -AsPlainText -Force
#$psCred = New-Object System.Management.Automation.PSCredential($ApplicationId , $securePassword)
#Connect-AzureAD -Credential $psCred -TenantId $TenantId

#Share Apps

$AppsToShare = ConvertFrom-Json $AppsToShareJson

foreach ($app in $AppsToShare.AppSharing)
{
	$AppName = $($app.AppName)
    Write-Verbose "Locating PowerApp: $AppName"

	#$AppId = (Get-AdminPowerApp $($app.AppName) -EnvironmentName $EnvironmentName).AppName
    #If ($AppId)
	#{
	#	Write-Verbose "PowerApp $AppName found with Id: $AppId"
	#}
	#else
	#{
	#	throw "$AppName could not be found"
	#}
    
    $records = Get-XrmEntities -ConnectionString $CrmConnectionString -EntityName "canvasapp" -Attribute "name" -Value "$AppName" -ConditionOperator 0

    if ($records.Count -eq 1)
    {
        $AppId = $records[0].Id
    }
    elseif ($records.Count -gt 1)
    {
        throw "Found mutiple Canvas Apps with name: $AppName"
    }
	else
	{
		throw "Canvas App with name: $AppName could not be found"
	}

	foreach($shareWith in $app.ShareWith)
	{
		$principalType = $shareWith.PrincipalType
        $principal = $shareWith.Principal
        $roleName = $shareWith.RoleName

        if ($principalType -eq "User")
		{
			$principalId = (Get-MgUser -UserId $principal | select Id).Id
		}
		elseif ($shareWith.PrincipalType -eq "Group")
		{
			$principalId = (Get-MgGroup -Filter "DisplayName eq '$principal'" | select Id).Id
		}
		else
		{
			$principalId = $TenantId
		}

        Write-Host "Sharing App '$AppName' [$AppId] with $principalType $principal [$principalId] access $roleName in Environment: $EnvironmentUrl [$EnvironmentId]"

		$response = Set-AdminPowerAppRoleAssignment -AppName $AppId -EnvironmentName $EnvironmentId -RoleName $roleName -PrincipalType $principalType -PrincipalObjectId $principalId

	    Write-Verbose "Invoked Set-AdminPowerAppRoleAssignment"

        $response

        if ($response.Error -or ($response.Code -ne '' -and $response.Code -ge 400))
        {
            throw "App Sharing Failed: $response"
        }

        Write-Host "Shared App"
    }
}
