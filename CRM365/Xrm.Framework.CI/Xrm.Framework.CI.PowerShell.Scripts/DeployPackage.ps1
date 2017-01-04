#
# DeployPackage.ps1
#

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

Write-Verbose 'Entering DeployPackage.ps1'

#Parameters
Write-Verbose "AuthenticationType = $AuthenticationType"
Write-Verbose "Username = $Username"
Write-Verbose "Password = $Password"
Write-Verbose "ServerUrl = $ServerUrl"
Write-Verbose "OrganizationName = $OrganizationName"
Write-Verbose "DeploymentRegion = $DeploymentRegion"
Write-Verbose "OnlineType = $OnlineType"
Write-Verbose "HomRealmURL = $HomRealmURL"
Write-Verbose "PackageName = $PackageName"
Write-Verbose "PackageDirectory = $PackageDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

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

Write-Verbose 'Leaving DeployPackage.ps1'
