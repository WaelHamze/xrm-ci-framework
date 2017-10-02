#
# BackupCRMOnlineInstance.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$InstanceId,
[string]$BackupLabel,
[string]$BackupNotes,
[bool]$IsAzureBackup = $false,
[string]$ContainerName,
[string]$StorageAccountKey,
[string]$StorageAccountName,
[bool]$WaitForCompletion = $false,
[int]$SleepDuration = 3,
[string]$PSModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering BackupCRMOnlineInstance.ps1'

#Parameters
Write-Verbose "ApiUrl = $ApiUrl"
Write-Verbose "Username = $Username"
Write-Verbose "InstanceId = $InstanceId"
Write-Verbose "BackupLabel = $BackupLabel"
Write-Verbose "BackupNotes = $BackupNotes"
Write-Verbose "IsAzureBackup = $IsAzureBackup"
Write-Verbose "ContainerName = $ContainerName"
Write-Verbose "StorageAccountKey = $StorageAccountKey"
Write-Verbose "StorageAccountName = $StorageAccountName"
Write-Verbose "WaitForCompletion = $WaitForCompletion"
Write-Verbose "SleepDuration = $SleepDuration"
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

$backupInfo = New-CrmBackupInfo -InstanceId $InstanceId -Label "$BackupLabel" -Notes "$BackupNotes" -IsAzureBackup $IsAzureBackup -AzureContainerName $ContainerName -AzureStorageAccountKey $StorageAccountKey -AzureStorageAccountName $StorageAccountName

#Create Credentials
$SecPassword = ConvertTo-SecureString $Password -AsPlainText -Force
$Cred = New-Object System.Management.Automation.PSCredential ($Username, $SecPassword)
 
$operation = Backup-CrmInstance -ApiUrl $ApiUrl -BackupInfo $backupInfo -Credential $Cred

$OperationId = $operation.OperationId
$OperationStatus = $operation.Status

Write-Output "OperationId = $OperationId"
Write-Verbose "Status = $OperationStatus"

if ($operation.Errors.Count -gt 0)
{
    $errorMessage = $operation.Errors[0].Description
    throw "Errors encountered : $errorMessage"
}

if ($WaitForCompletion)
{
	& "$scriptPath\WaitForCRMOperation.ps1" -OperationId $OperationId -PSModulePath $PSModulePath
}

Write-Verbose 'Leaving BackupCRMOnlineInstance.ps1'

