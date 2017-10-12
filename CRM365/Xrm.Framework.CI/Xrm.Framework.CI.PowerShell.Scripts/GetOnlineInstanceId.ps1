#
# GetOnlineInstanceId.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$DomainName,
[string]$VstsInstanceIdVariableName,
[string]$PSModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering GetOnlineInstanceId.ps1'

#Parameters
Write-Verbose "ApiUrl = $ApiUrl"
Write-Verbose "Username = $Username"
Write-Verbose "DomainName = $DomainName"
Write-Verbose "VstsInstanceIdVariableName = $VstsInstanceIdVariableName"
Write-Verbose "PSModulePath = $PSModulePath"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load Online Management Module
$xrmOnlineModule = $scriptPath + "\Microsoft.Xrm.OnlineManagementAPI.dll"

if ($PSModulePath)
{
	$xrmOnlineModule = $PSModulePath + "\Microsoft.Xrm.OnlineManagementAPI.dll"
}

Write-Verbose "Importing Online Management Module: $xrmOnlineModule"
Import-Module $xrmOnlineModule
Write-Verbose "Imported Online Management Module"

#Create Credentials
$SecPassword = ConvertTo-SecureString $Password -AsPlainText -Force
$Cred = New-Object System.Management.Automation.PSCredential ($Username, $SecPassword)

$instances = Get-CrmInstances -ApiUrl $ApiUrl -Credential $Cred

$id = $null

foreach ($instance in $instances)
{
	if ($instance.DomainName -eq $DomainName)
	{
		$id = $instance.Id
		break
	}
}

If ($id -ne $null)
{
	Write-Host "##vso[task.setvariable variable=$VstsInstanceIdVariableName]$id"
}
else
{
	throw "Instance with domain name of $DomainName not found."
}

Write-Verbose 'Leaving GetOnlineInstanceId.ps1'