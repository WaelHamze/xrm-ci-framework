#
# RestoreOnlineInstance.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$SourceInstanceName,
[string]$BackupLabel,
[string]$RestoreTimeUtc,
[string]$TargetInstanceName,
[string]$FriendlyName,
[string]$SecurityGroupId,
[string]$SecurityGroupName,
[int]$MaxCrmConnectionTimeOutMinutes,
[bool]$WaitForCompletion = $false,
[int]$SleepDuration = 3,
[string]$PSModulePath,
[string]$AzureADModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering RestoreOnlineInstance.ps1'

#Parameters
Write-Verbose "ApiUrl = $ApiUrl"
Write-Verbose "Username = $Username"
Write-Verbose "SourceInstanceName = $SourceInstanceName"
Write-Verbose "BackupLabel = $BackupLabel"
Write-Verbose "TargetInstanceName = $TargetInstanceName"
Write-Verbose "FriendlyName = $FriendlyName"
Write-Verbose "SecurityGroupId = $SecurityGroupId"
Write-Verbose "SecurityGroupName = $SecurityGroupName"
Write-Verbose "MaxCrmConnectionTimeOutMinutes = $MaxCrmConnectionTimeOutMinutes"
Write-Verbose "WaitForCompletion = $WaitForCompletion"
Write-Verbose "SleepDuration = $SleepDuration"
Write-Verbose "PSModulePath = $PSModulePath"
Write-Verbose "AzureADModulePath = $AzureADModulePath"

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

if ($BackupLabel)
{
    $instanceBackup = Get-XrmBackupByLabel -ApiUrl $ApiUrl -Cred $Cred -InstanceId $sourceInstance.Id -Label $BackupLabel

    if ($instanceBackup -eq $null)
    {
        throw "Backup with label $BackupLabel not found"
    }

    $RestoreTimeUtc = $instanceBackup.TimestampUtc

    Write-Host "Restoring backup with label: $BackupLabel"
    Write-Host "Backup Utc Timestamp: $RestoreTimeUtc"
}
else
{
    Write-Host "Restoring backup Timestamp: $RestoreTimeUtc"
}

if ($SecurityGroupName)
{
	if ($AzureADModulePath)
    {
        Write-Verbose "Importing Module AzureAD" 
	    Import-Module "$AzureADModulePath\AzureAD.psd1"
	    Write-Verbose "Imported Module AzureAD"

        Connect-AzureAD -Credential $Cred
	
	    $group = Get-AzureADGroup -Filter "DisplayName eq '$SecurityGroupName'"

	    if ($group)
	    {
		    Write-Host "Security Group Found with Id $($group.ObjectId)"
            $SecurityGroupId = $group.ObjectId
	    }
	    else
	    {
		    throw "$SecurityGroupName not found"
	    }
    }
    else
    {
        throw "AzureADModulePath is required"
    }
}

$RestoreOffSet = [System.DateTimeOffset]$RestoreTimeUtc

$CallParams = @{
	ApiUrl = $ApiUrl
	Credential = $Cred
	SourceInstanceId = $sourceInstance.Id
	TargetInstanceId = $targetInstance.Id
    RestoreTimeUtc = $RestoreOffSet
}

if ($FriendlyName)
{
	$CallParams.FriendlyName = $FriendlyName
}

if ($SecurityGroupId)
{
	$CallParams.SecurityGroupId = $SecurityGroupId
}

if ($MaxCrmConnectionTimeOutMinutes -and ($MaxCrmConnectionTimeOutMinutes -ne 0))
{
	$CallParams.MaxCrmConnectionTimeOutMinutes = $MaxCrmConnectionTimeOutMinutes
}

$operation = Restore-CrmInstance @CallParams

#$operation = Restore-CrmInstance -ApiUrl $ApiUrl -Credential $Cred -BackupId $instanceBackup.Id -SourceInstanceId $sourceInstance.Id -TargetInstanceId $targetInstance.Id

$OperationId = $operation.OperationId
$OperationStatus = $operation.Status

Write-Output "OperationId = $OperationId"
Write-Output "Status = $OperationStatus"

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