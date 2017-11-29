#
# RestoreOnlineInstance.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$SourceInstanceName,
[string]$BackupLabel,
[string]$TargetInstanceName,
[bool]$WaitForCompletion = $false,
[int]$SleepDuration = 3,
[string]$PSModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering RestoreOnlineInstance.ps1'

#Parameters
Write-Verbose "ApiUrl = $ApiUrl"
Write-Verbose "Username = $Username"
Write-Verbose "SourceInstanceName = $SourceInstanceName"
Write-Verbose "BackupLabel = $BackupLabel"
Write-Verbose "TargetInstanceName = $TargetInstanceName"
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

$sourceInstance = Get-XrmInstanceByName -ApiUrl $ApiUrl -Cred $Cred -InstanceName $SourceInstanceName

if ($sourceInstance -eq $null)
{
    throw "$SourceInstanceName not found"
}

$targetInstance = Get-XrmInstanceByName -ApiUrl $ApiUrl -Cred $Cred -InstanceName $TargetInstanceName

if ($targetInstance -eq $null)
{
    throw "$TargetInstanceName not found"
}

$instanceBackup = Get-XrmBackupByLabel -ApiUrl $ApiUrl -Cred $Cred -InstanceId $sourceInstance.Id -Label $BackupLabel

if ($instanceBackup -eq $null)
{
    throw "Backup with label $BackupLabel not found"
}

$operation = Restore-CrmInstance -ApiUrl $ApiUrl -Credential $Cred -BackupId $instanceBackup.Id -SourceInstanceId $sourceInstance.Id -TargetInstanceId $targetInstance.Id

if ($operation.Errors.Count -gt 0)
{
    $errorMessage = $operation.Errors[0].Description
    throw "Errors encountered : $errorMessage"
}

$status = Wait-XrmOperation -ApiUrl $ApiUrl -Cred $Cred -operationId $operation.OperationId

$status

if ($status.Status -ne "Succeeded")
{
	throw "Operation status: $status.Status"
}

Write-Verbose 'Leaving RestoreOnlineInstance.ps1'