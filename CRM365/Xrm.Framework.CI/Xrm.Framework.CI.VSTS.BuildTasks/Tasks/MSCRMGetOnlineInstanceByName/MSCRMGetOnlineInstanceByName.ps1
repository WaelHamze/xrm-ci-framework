[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMGetOnlineInstanceByName.ps1'

$apiUrl = Get-VstsInput -Name apiUrl -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$instanceName = Get-VstsInput -Name instanceName -Require
$vstsInstanceIdOutputVariableName = Get-VstsInput -Name vstsInstanceIdOutputVariableName -Require

#Print Verbose
Write-Verbose "apiUrl = $apiUrl"
Write-Verbose "username = $username"
Write-Verbose "instanceName = $instanceName"
Write-Verbose "vstsInstanceIdOutputVariableName = $vstsInstanceIdOutputVariableName"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load Online Management Module
$PSModulePath = "$scriptPath\ps_modules\Microsoft.Xrm.OnlineManagementAPI"

$xrmOnlineModule = $PSModulePath + "\Microsoft.Xrm.OnlineManagementAPI.dll"

Write-Verbose "Importing Online Management Module: $xrmOnlineModule"
Import-Module $xrmOnlineModule
Write-Verbose "Imported Online Management Module"

. "$scriptPath\ps_modules\xRMCIFramework\OnlineInstanceFunctions.ps1"

$secPassword = ConvertTo-SecureString $password -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential ($username, $secPassword)

$id = $null
$output = Get-XrmInstanceByName -ApiUrl $apiUrl -Cred $cred -InstanceName $instanceName
$id = $output.Id

If ($id -ne $null)
{
	Write-Host "##vso[task.setvariable variable=$vstsInstanceIdOutputVariableName]$id"
}
else
{
	throw "Instance with name '$instanceName' not found."
}

Write-Verbose 'Leaving MSCRMGetOnlineInstanceByName.ps1'
