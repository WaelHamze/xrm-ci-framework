[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMSetOnlineInstanceAdminMode.ps1'

#Get Parameters
$apiUrl = Get-VstsInput -Name apiUrl -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$instanceName = Get-VstsInput -Name instanceName -Require
$enable = Get-VstsInput -Name enable -Require -AsBool
$allowBackgroundOperations = Get-VstsInput -Name allowBackgroundOperations -AsBool
$notificationText = Get-VstsInput -Name notificationText

#Print Verbose
Write-Verbose "apiUrl = $apiUrl"
Write-Verbose "username = $username"
Write-Verbose "instanceName = $instanceName"
Write-Verbose "Enable = $enable"
Write-Verbose "AllowBackgroundOperations = $allowBackgroundOperations"
Write-Verbose "NotificationText = $notificationText"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

$PSModulePath = "$scriptPath\ps_modules\Microsoft.Xrm.OnlineManagementAPI"

& "$scriptPath\SetOnlineInstanceAdminMode.ps1" -ApiUrl $apiUrl -Username $username -Password $password -InstanceName $instanceName  -Enable $enable -AllowBackgroundOperations $allowBackgroundOperations -NotificationText $notificationText -PSModulePath $PSModulePath -WaitForCompletion $true -SleepDuration 5

Write-Verbose 'Leaving MSCRMSetOnlineInstanceAdminMode.ps1'
