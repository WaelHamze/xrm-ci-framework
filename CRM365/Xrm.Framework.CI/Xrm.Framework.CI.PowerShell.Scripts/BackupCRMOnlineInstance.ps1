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
Write-Verbose "WaitForCompletion = $WaitForCompletion"
Write-Verbose "SleepDuration = $SleepDuration"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load Online Management Module
$xrmOnlineModule = $scriptPath + "Microsoft.Xrm.OnlineManagementAPI.dll"

if ($PSModulePath)
{
	$xrmOnlineModule = $PSModulePath + "Microsoft.Xrm.OnlineManagementAPI.dll"
}

Write-Verbose "Importing Online Management Module: $xrmOnlineModule" 
Import-Module $xrmOnlineModule
Write-Verbose "Imported Online Management Module"

$backupInfo = New-CrmBackupInfo -InstanceId $InstanceId -Label "$BackupLabel" -IsAzureBackup $false -Notes "$BackupNotes"

#Create Credentials
$SecPassword = ConvertTo-SecureString $Password -AsPlainText -Force
$Cred = New-Object System.Management.Automation.PSCredential ($Username, $SecPassword)
 
$backupOutput = Backup-CrmInstance -ApiUrl $ApiUrl -BackupInfo $backupInfo -Credential $Cred

$OperationId = $backupOutput.OperationId
$OperationStatus = $backupOutput.Status

Write-Output "OperationId = $OperationId"
Write-Verbose "Status = $OperationStatus"

if ($WaitForCompletion)
{
	$completed = $false
	Write-Verbose "Waiting for completion...$OperationId"

	while ($completed -eq $false)
	{	
		Start-Sleep -Seconds $SleepDuration
    
		$OpStatus = Get-CrmOperationStatus -ApiUrl $ApiUrl -Credential $Cred -Id $OperationId

		$OperationStatus = $OpStatus.Status
    
		Write-Verbose "Status = $OperationStatus"
    
		if ($OperationStatus -eq "Succeeded")
		{
			$completed = $true
		}
	}
}

# NotStarted, Running, Ready, Succeeded