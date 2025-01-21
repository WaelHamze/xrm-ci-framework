#
# AddEnvironmentToGroup.ps1
#

[CmdletBinding()]

param(
	[string]$TenantId , #The tenant Id where your instance resides
	[string]$ApplicationId , #The application Id used for the connection
	[string]$ApplicationSecret, #The application secret used for connection
    [string]$Username,
    [string]$Password,
	#[string]$EnvironmentUrl,
    #[string]$EnvironmentId,
    [string]$Environment,
	[string]$GroupId,
    [string]$PowerAppsCLIPath
	#[string]$MSALModulePath,
    #[string]$CrmConnectorModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering AddEnvironmentToGroup.ps1'

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Import Modules

#$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
#Write-Verbose "Importing: $xrmCIToolkit"
#Import-Module $xrmCIToolkit

#Write-Verbose "Import Crm Connector: $CrmConnectorModulePath"
#Import-module "$CrmConnectorModulePath\Microsoft.Xrm.Tooling.CrmConnector.PowerShell.psd1"

#Write-Verbose "Importing MSAL Module" 
#Import-Module "$MSALModulePath\MSAL.PS.psd1"

#Write-Verbose "Importing PowerApps Admin Module: $PowerAppsAdminModulePath"
#Import-module "$PowerAppsAdminModulePath\Microsoft.PowerApps.Administration.PowerShell.psd1"

Write-Verbose "Power Apps CLI Path: $PowerAppCLIPath"

if ($PowerAppsCLIPath)
{
    if (-not $PowerAppsCLIPath.EndsWith('\'))
    {
        $PowerAppsCLIPath = $PowerAppsCLIPath + '\'
        Write-Verbose "Adjusted Power Apps CLI Path: $PowerAppCLIPath"
    }
}

$PAC = "$($PowerAppsCLIPath)pac"

$conName = "PowerDevOps-SPN-Connection"

if ($ApplicationId)
{
    & "$PAC" auth create --name $conName --applicationId $ApplicationId --clientSecret $ApplicationSecret --tenant $TenantId
}
elseif ($Username)
{
    & "$PAC" auth create --name $conName --username $Username --password $Password
}
else
{
    throw "Either Username/Password or ApplicationId/ClientSecret must be provided"
}

try
{
    $res = & "$PAC" admin add-group --environment-group $GroupId --environment $Environment
}
catch
{
    Write-Host "$_"
    throw "Error when adding environment to group $_"
}
finally
{
    & "$PAC" auth clear
}

$resText = $res -join "`r`n"

Write-Verbose $resText

if ($resText.Contains('Error'))
{
    throw "Adding environment $EnvironmentId to group $GroupId failed. $resText"
}
elseif ($resText.Contains('Done'))
{
    Write-Host "Environment added to Group"
}
else
{
    Write-Warning "Could not process response $resText"
}

#Connect

#Write-Verbose "Connecting to PowerApps Endpoint"
#Add-PowerAppsAccount -TenantID $TenantId -ApplicationId $ApplicationId -ClientSecret $ApplicationSecret -Endpoint prod
#Add-PowerAppsAccount -TenantID $TenantId -Username $Username -Password ($Password | ConvertTo-SecureString -AsPlainText -Force) -Endpoint prod

#Get-AdminPowerAppEnvironment

#Write-Verbose "Connecting to Microsoft Graph"
#$scopes = @('https://api.powerplatform.com/.default')
#$MsalToken = Get-MsalToken -TenantId $TenantId -ClientId $ApplicationId -ClientSecret ($ApplicationSecret | ConvertTo-SecureString -AsPlainText -Force) -Scopes $scopes
#$accessToken = $MsalToken.AccessToken

#$token = Get-JwtToken -Audience "https://api.powerplatform.com/"
#$accessToken = $token

#$authRes = Invoke-RestMethod -Method Post -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" -ContentType "application/x-www-form-urlencoded" -Body "client_id=$ApplicationId&scope=https://api.powerplatform.com/.default&client_secret=$ApplicationSecret&grant_type=client_credentials"
#$body = "client_id=$ApplicationId&scope=https://api.powerplatform.com/.default&username=$Username&password=$Password&grant_type=password"
#$authRes = Invoke-RestMethod -Method Post -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" -ContentType "application/x-www-form-urlencoded" -Body $body


#$accessToken = $authRes.access_token

#$GetGroupUrl = "https://api.powerplatform.com/environmentmanagement/environmentGroups/{$($GroupId)}?api-version=2022-03-01-preview"
#$GetGroupUrl = "https://api.powerplatform.com/analytics/advisorRecommendations?api-version=2022-03-01-preview"
#$GetGroupUrl = "https://api.powerplatform.com/appmanagement/applicationPackages?api-version=2022-03-01-preview" #Ok
#$GetGroupUrl = "https://api.powerplatform.com/environmentmanagement/environmentGroups?api-version=2022-03-01-preview"
#$GetGroupUrl = "https://api.powerplatform.com/environmentmanagement/environmentGroups?api-version=1"

#$accessToken = $token
#$headers = @{Authorization = "Bearer $accessToken" }
#'Content-Type' = "application/json"

#$environmentGroup = Invoke-RestMethod -Uri $GetGroupUrl -Headers $headers -Method Get

#if ($environmentGroup)
#{
#    $groupName = $environmentGroup.displayName
#
#   If ($groupName)
#    {
#        Write-Host "Add environment $EnvironmentId to Group '$groupName'"
#    }
#}
#else
#{
#    throw "Environment Group with ID $GroupId was not found"
#}

#$AddToGroupUrl = "https://api.powerplatform.com/environmentmanagement/environmentGroups/$GroupId/addEnvironment/$($EnvironmentId)?api-version=2022-03-01-preview"

#$addResponse = Invoke-WebRequest -Uri $AddToGroupUrl -Method Post -Headers $headers

#if ($addResponse.StatusCode -eq 202)
#{
#    Write-Host "Add environment $EnvironmentId to Group '$groupName' ID $GroupId"
#}
#else
#{
#    Write-Host "Add operation failed. Response below:"
#    Write-Host "Response: $($addResponse.RawContent)"
#    throw "Add operation has failed"
#}

#$CrmConnectionString = "AuthType=ClientSecret;url=$EnvironmentUrl;ClientId=$ApplicationId;ClientSecret=$ApplicationSecret"
#$CRMConn = Get-CrmConnection -ConnectionString $CrmConnectionString -Verbose
#$EnvironmentId = $CRMConn.EnvironmentId

#Azure AD

#$securePassword = ConvertTo-SecureString $ApplicationSecret -AsPlainText -Force
#$psCred = New-Object System.Management.Automation.PSCredential($ApplicationId , $securePassword)
#Connect-AzureAD -Credential $psCred -TenantId $TenantId

Write-Verbose 'Leaving AddEnvironmentToGroup.ps1'