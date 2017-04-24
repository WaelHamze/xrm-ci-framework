param(
[string]$AuthenticationType,
[string]$Username,
[string]$Password,
[string]$ServerUrl,
[string]$OrganizationName,
[string]$DeploymentRegion,
[string]$OnlineType,
[string]$HomRealmURL,
[string]$PackageName,
[string]$PackageDirectory
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "AuthenticationType: $AuthenticationType"
Write-Host "Username: $Username"
Write-Host "Password: $Password"
Write-Host "ServerUrl: $ServerUrl"
Write-Host "OrganizationName: $OrganizationName"
Write-Host "DeploymentRegion: $DeploymentRegion"
Write-Host "OnlineType: $OnlineType"
Write-Host "HomRealmURL: $HomRealmURL"
Write-Host "PackageName: $PackageName"
Write-Host "PackageDirectory: $PackageDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

#Load XRM Tooling

Add-PSSnapin Microsoft.Xrm.Tooling.Connector

Add-PSSnapin Microsoft.Xrm.Tooling.PackageDeployment

#Create Credentials
$SecPassword = ConvertTo-SecureString $Password -AsPlainText -Force
$Cred = New-Object System.Management.Automation.PSCredential ($Username, $SecPassword)

#Create Connection

switch($AuthenticationType)
{
    "AD" { $CRMConn = Get-CrmConnection -ServerUrl $ServerUrl -OrganizationName $OrganizationName -Credential $Cred }
    "Claims" { $CRMConn = Get-CrmConnection -ServerUrl $ServerUrl -OrganizationName $OrganizationName -Credential $Cred –HomRealmURL $HomRealmURL}
    "IFD" { $CRMConn = Get-CrmConnection -ServerUrl $ServerUrl -OrganizationName $OrganizationName -Credential $Cred }
    "Live" { $CRMConn = Get-CrmConnection -Credential $Cred -DeploymentRegion $DeploymentRegion –OnlineType $OnlineType –OrganizationName $OrganizationName }
    "Office365" { $CRMConn = Get-CrmConnection -Credential $Cred -DeploymentRegion $DeploymentRegion –OnlineType $OnlineType –OrganizationName $OrganizationName }
}

#Deploy Package

Import-CrmPackage –CrmConnection $CRMConn –PackageDirectory $PackageDirectory –PackageName $PackageName -Verbose