#
# ShareFlows.ps1
#
[CmdletBinding()]

param(
	[string]$TenantId , #The tenant Id where your instance resides
	[string]$ApplicationId , #The application Id used for the connection
	[string]$ApplicationSecret, #The application secret used for connection
	[string]$EnvironmentUrl,
	[string]$FlowsToShareJson,
	[string]$MSALModulePath,
	[string]$MGUsersModulePath,
	[string]$MGGroupsModulePath,
	[string]$PowerAppsModulePath,
	[string]$PowerAppsAdminModulePath,
    [string]$CrmConnectorModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ShareFlows.ps1'

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

$FlowsToShare = ConvertFrom-Json $FlowsToShareJson

foreach ($flow in $FlowsToShare.FlowSharing)
{
	$FlowName = $($flow.FlowName)
    Write-Verbose "Locating Flow: $FlowName"

	#$AppId = (Get-AdminPowerApp $($app.AppName) -EnvironmentName $EnvironmentName).AppName
    #If ($AppId)
	#{
	#	Write-Verbose "PowerApp $AppName found with Id: $AppId"
	#}
	#else
	#{
	#	throw "$AppName could not be found"
	#}
    
    $records = Get-XrmEntities -ConnectionString $CrmConnectionString -FetchXml "<fetch top='50'><entity name='workflow'><attribute name='workflowid'/><attribute name='name'/><attribute name='category'/><attribute name='workflowidunique'/><attribute name='ownerid' /><filter type='and' ><condition attribute='name' operator='eq' value='$FlowName' /><condition attribute='category' operator='eq' value='5' /></filter></entity></fetch>"

    if ($records.Count -eq 1)
    {
        $FlowId = $records[0].workflowidunique
    }
    elseif ($records.Count -gt 1)
    {
        throw "Found mutiple Flows with name: $FlowName"
    }
	else
	{
		throw "Flow with name: $FlowName could not be found"
	}

	foreach($shareWith in $flow.ShareWith)
	{
		$principalType = $shareWith.PrincipalType
        $principal = $shareWith.Principal
        $roleName = $shareWith.RoleName

        if ($principalType -eq "User")
		{
			$principalId = (Get-MgUser -UserId $principal | select Id).Id
			$users = Get-XrmEntities -ConnectionString $CrmConnectionString -FetchXml "<fetch top='50' ><entity name='systemuser' ><attribute name='azureactivedirectoryobjectid' /><attribute name='domainname' /><attribute name='systemuserid' /><filter><condition attribute='domainname' operator='eq' value='$principal' /></filter></entity></fetch>"

			$userId = $users[0].Id
		}
		elseif ($shareWith.PrincipalType -eq "Group")
		{
			$principalId = (Get-MgGroup -Filter "DisplayName eq '$principal'" | select Id).Id
		}
		else
		{
			$principalId = $TenantId
		}

		if ($roleName -eq "Owner")
		{
			Write-Host "Assigning Flow '$FlowName' [$FlowId] with $principalType $principal [$principalId] access $roleName in Environment: $EnvironmentUrl [$EnvironmentId]"

			if ($records[0].ownerid.Id -ne $userId)
			{
				$assign = New-XrmEntity -EntityName 'workflow'
				$assign.Id = $records[0].Id
				$owner = New-Object -TypeName Microsoft.Xrm.Sdk.EntityReference -ArgumentList 'systemuser'
				$owner.LogicalName = 'systemuser'
				$owner.Id = $records[0].ownerid.Id
				$assign.Attributes.add('ownerid', $owner);
				Set-XrmEntity -ConnectionString $CrmConnectionString -EntityObject ($assign)

				Write-Host "Changed Flow Owner"
			}
			else
			{
				Write-Host "Skipped as current Owner is $($records[0].ownerid.Id)"
			}
		}
		else
		{
			Write-Host "Sharing Flow '$FlowName' [$FlowId] with $principalType $principal [$principalId] access $roleName in Environment: $EnvironmentUrl [$EnvironmentId]"

			$response = Set-AdminFlowOwnerRole -FlowName $FlowId -EnvironmentName $EnvironmentId -RoleName $roleName -PrincipalType $principalType -PrincipalObjectId $principalId

			Write-Verbose "Invoked Set-AdminFlowOwnerRole"

			$response

			if ($response.Error -or ($response.Code -ne '' -and $response.Code -ge 400))
			{
				throw "Flow Sharing Failed: $response"
			}
		}

        Write-Host "Shared Flow"
    }
}
