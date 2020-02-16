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
[int]$MaxCrmConnectionTimeOutMinutes,
[bool]$WaitForCompletion = $false,
[int]$SleepDuration = 3,
[string]$PSModulePath,
[string]$BackupExistsAction = "Error" #Error,Skip,Continue
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering BackupCRMOnlineInstance.ps1'

#Parameters
Write-Verbose "ApiUrl = $ApiUrl"
Write-Verbose "Username = $Username"
Write-Verbose "InstanceName = $InstanceName"
Write-Verbose "BackupLabel = $BackupLabel"
Write-Verbose "BackupNotes = $BackupNotes"
Write-Verbose "MaxCrmConnectionTimeOutMinutes = $MaxCrmConnectionTimeOutMinutes"
Write-Verbose "WaitForCompletion = $WaitForCompletion"
Write-Verbose "SleepDuration = $SleepDuration"
Write-Verbose "PSModulePath = $PSModulePath"
Write-Verbose "BackupExistsAction = $BackupExistsAction"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Set Security Protocol
& "$scriptPath\SetTlsVersion.ps1"

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

$backup = Get-XrmBackupByLabel -ApiUrl $ApiUrl -Cred $Cred -InstanceId $instance.Id -Label "$BackupLabel"

if ($backup)
{
	Write-Host "Existing Backup with same label found. Timestamp: $($backup.TimestampUtc)"
	
	if ($BackupExistsAction -eq "Error")
	{
		throw "Backup with label $BackupLabel already exists for instance"
	}
	elseif ($BackupExistsAction -eq "Skip")
	{
		Write-Warning "Skipping backup as backup with same label already exists"
		return
	}
	elseif ($BackupExistsAction -eq "Continue")
	{
		Write-Host "Will continue to attempt another backup using same label"
	}
}
else
{
	Write-Host "No existing backups found with label: $BackupLabel"
}

#$backupInfo = New-CrmBackupInfo -InstanceId $instance.Id -Label "$BackupLabel" -Notes "$BackupNotes" -IsAzureBackup $IsAzureBackup -AzureContainerName $ContainerName -AzureStorageAccountKey $StorageAccountKey -AzureStorageAccountName $StorageAccountName
 
$CallParams = @{
	ApiUrl = $ApiUrl
	Credential = $Cred
	InstanceId = $instance.Id
	Label = "$BackupLabel"
    Notes = "$BackupNotes"
}

if ($MaxCrmConnectionTimeOutMinutes -and ($MaxCrmConnectionTimeOutMinutes -ne 0))
{
	$CallParams.MaxCrmConnectionTimeOutMinutes = $MaxCrmConnectionTimeOutMinutes
}

$operation = Backup-CrmInstance @CallParams

$operation

if ($operation.Errors.Count -gt 0)
{
    $errorMessage = $operation.Errors[0].Description
    throw "Errors encountered : $errorMessage"
}

if ($WaitForCompletion -and ($operation.OperationId -ne [system.guid]::empty) -and ($OperationStatus -ne "Succeeded") -and ($OperationStatus -ne "Created"))
{
	$status = Wait-XrmOperation -ApiUrl $ApiUrl -Cred $Cred -operationId $operation.OperationId

	$status

	if ($status.Status -ne "Succeeded")
	{
		throw "Operation status: $status.Status"
	}

	Wait-XrmBackup -ApiUrl $ApiUrl -Cred $Cred -InstanceId $instance.Id -Label "$BackupLabel" -SleepDuration 15

	Write-Host "Backup Completed"
}

Write-Verbose 'Leaving BackupCRMOnlineInstance.ps1'
