#
# BackupCRMOnlineInstance.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$InstanceName,
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
Write-Verbose "InstanceName = $InstanceName"
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

#Create Credentials
$SecPassword = ConvertTo-SecureString $Password -AsPlainText -Force
$Cred = New-Object System.Management.Automation.PSCredential ($Username, $SecPassword)

."$scriptPath\OnlineInstanceFunctions.ps1"

$instance = Get-XrmInstanceByName -ApiUrl $ApiUrl -Cred $Cred -InstanceName $InstanceName

if ($instance -eq $null)
{
    throw "$InstanceName not found"
}

Write-Host "Backing up instance $InstanceName " + $instance.Id

$backupInfo = New-CrmBackupInfo -InstanceId $instance.Id -Label "$BackupLabel" -Notes "$BackupNotes" -IsAzureBackup $IsAzureBackup -AzureContainerName $ContainerName -AzureStorageAccountKey $StorageAccountKey -AzureStorageAccountName $StorageAccountName
 
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
	$status = Wait-XrmOperation -ApiUrl $ApiUrl -Cred $Cred -operationId $operation.OperationId

	$status

	if ($status.Status -ne "Succeeded")
	{
		throw "Operation status: $status.Status"
	}
}

Write-Verbose 'Leaving BackupCRMOnlineInstance.ps1'
