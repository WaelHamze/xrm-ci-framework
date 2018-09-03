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

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

#Load Online Management Module
$PSModulePath = "$mscrmToolsPath\OnlineManagementAPI\1.0.0"

$xrmOnlineModule = $PSModulePath + "\Microsoft.Xrm.OnlineManagementAPI.dll"

Write-Verbose "Importing Online Management Module: $xrmOnlineModule"
Import-Module $xrmOnlineModule
Write-Verbose "Imported Online Management Module"

. "$mscrmToolsPath\xRMCIFramework\9.0.0\OnlineInstanceFunctions.ps1"

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
