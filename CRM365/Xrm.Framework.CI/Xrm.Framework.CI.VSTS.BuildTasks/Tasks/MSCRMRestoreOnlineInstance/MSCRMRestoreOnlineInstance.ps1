[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMDeleteOnlineInstance.ps1'

$apiUrl = Get-VstsInput -Name apiUrl -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$sourceInstanceName = Get-VstsInput -Name sourceInstanceName -Require
$backupLabel = Get-VstsInput -Name backupLabel -Require
$targetInstanceName = Get-VstsInput -Name targetInstanceName -Require
$waitForCompletion = Get-VstsInput -Name waitForCompletion -AsBool
$sleepDuration = Get-VstsInput -Name sleepDuration -AsInt

#Print Verbose
Write-Verbose "apiUrl = $apiUrl"
Write-Verbose "username = $username"
Write-Verbose "sourceInstanceName = $sourceInstanceName"
Write-Verbose "backupLabel = $backupLabel"
Write-Verbose "targetInstanceName = $targetInstanceName"
Write-Verbose "waitForCompletion = $waitForCompletion"
Write-Verbose "sleepDuration = $sleepDuration"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

$PSModulePath = "$scriptPath\ps_modules\Microsoft.Xrm.OnlineManagementAPI"

& "$scriptPath\RestoreOnlineInstance.ps1" -ApiUrl $apiUrl -Username $username -Password $password -sourceInstanceName $sourceInstanceName  -BackupLabel $backupLabel -targetInstanceName $targetInstanceName -PSModulePath $PSModulePath -WaitForCompletion $WaitForCompletion -SleepDuration $sleepDuration

Write-Verbose 'Leaving MSCRMRestoreOnlineInstance.ps1'
