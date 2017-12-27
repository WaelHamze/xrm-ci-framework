[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMDeleteOnlineInstance.ps1'

$apiUrl = Get-VstsInput -Name apiUrl -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$instanceName = Get-VstsInput -Name instanceName -Require
$waitForCompletion = Get-VstsInput -Name waitForCompletion -AsBool
$sleepDuration = Get-VstsInput -Name sleepDuration -AsInt

#Print Verbose
Write-Verbose "apiUrl = $apiUrl"
Write-Verbose "username = $username"
Write-Verbose "instanceName = $instanceName"
Write-Verbose "waitForCompletion = $waitForCompletion"
Write-Verbose "sleepDuration = $sleepDuration"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

$PSModulePath = "$scriptPath\ps_modules\Microsoft.Xrm.OnlineManagementAPI"

& "$scriptPath\DeleteOnlineInstance.ps1" -ApiUrl $apiUrl -Username $username -Password $password  -InstanceName $InstanceName -PSModulePath $PSModulePath -WaitForCompletion $WaitForCompletion -SleepDuration $sleepDuration

Write-Verbose 'Leaving MSCRMDeleteOnlineInstance.ps1'
