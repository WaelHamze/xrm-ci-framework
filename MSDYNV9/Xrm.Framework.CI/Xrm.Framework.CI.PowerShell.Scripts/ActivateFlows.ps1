#
# ActivateFlows.ps1
#
[CmdletBinding()]

param(
	[string]$TenantId , #The tenant Id where your instance resides
	[string]$ApplicationId , #The application Id used for the connection
	[string]$ApplicationSecret, #The application secret used for connection
	[string]$EnvironmentUrl,
	[string]$FlowsToActivateJson,
	[string]$PowerAppsModulePath,
	[string]$PowerAppsAdminModulePath,
    [string]$CrmConnectorModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ActivateFlows.ps1'

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

#Activate Flows

$FlowsToActivate = ConvertFrom-Json $FlowsToActivateJson

foreach ($flow in $FlowsToActivate.Flows)
{
	$FlowName = $($flow.FlowName)
    Write-Verbose "Locating Flow: $FlowName"
    
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

	$state = $flow.State

	if ($state -eq "On")
	{
		$stateCode = 1
		$statusCode = 2
	}
	elseif ($state -eq "Off")
	{
		$stateCode = 0
		$statusCode = 1
	}
	else
	{
		throw "$state is not supported"
	}

	Set-XrmEntityState -ConnectionString $CrmConnectionString -EntityName "workflow" -Id $records[0].Id -StateCode $stateCode -StatusCode $statusCode
}

Write-Verbose 'Leaving ActivateFlows.ps1'