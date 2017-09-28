[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMBackupOnlineInstance.ps1'

#Get Parameters
$apiUrl = Get-VstsInput -Name apiUrl -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$instanceId = Get-VstsInput -Name instanceId -Require
$backupLabel = Get-VstsInput -Name backupLabel -Require
$backupNotes = Get-VstsInput -Name backupNotes
$isAzureBackup = Get-VstsInput -Name isAzureBackup -AsBool -Require
$containerName = Get-VstsInput -Name containerName
$storageAccountKey = Get-VstsInput -Name storageAccountKey
$storageAccountName = Get-VstsInput -Name storageAccountName
$waitForCompletion = Get-VstsInput -Name waitForCompletion -AsBool
$sleepDuration = Get-VstsInput -Name sleepDuration -AsInt

#Print Verbose
Write-Verbose "apiUrl = $apiUrl"
Write-Verbose "username = $username"
Write-Verbose "instanceId = $instanceId"
Write-Verbose "backupLabel = $backupLabel"
Write-Verbose "backupNotes = $backupNotes"
Write-Verbose "isAzureBackup = $isAzureBackup"
Write-Verbose "containerName = $containerName"
Write-Verbose "storageAccountKey = $storageAccountKey"
Write-Verbose "storageAccountName = $storageAccountName"
Write-Verbose "waitForCompletion = $waitForCompletion"
Write-Verbose "sleepDuration = $sleepDuration"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

$PSModulePath = "$scriptPath\ps_modules\Microsoft.Xrm.OnlineManagementAPI"

& "$scriptPath\ps_modules\xRMCIFramework\BackupOnlineInstance.ps1" -ApiUrl $apiUrl -Username $username -Password $password -InstanceId $instanceId -BackupLabel $backupLabel -BackupNotes $backupNotes -WaitForCompletion $waitForCompletion -SleepDuration $sleepDuration -PSModulePath $PSModulePath  -IsAzureBackup $isAzureBackup -ContainerName $containerName -StorageAccountKey $storageAccountKey -StorageAccountName $storageAccountName

Write-Verbose 'Leaving MSCRMBackupOnlineInstance.ps1'
