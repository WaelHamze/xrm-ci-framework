#
# BackupCRMOnlineInstance.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$InstanceId,
[string]$InstanceName,
[bool]$WaitForCompletion = $false,
[int]$SleepDuration = 3,
[string]$PSModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering DeleteOnlineInstance.ps1'

#Parameters
Write-Verbose "ApiUrl = $ApiUrl"
Write-Verbose "Username = $Username"
Write-Verbose "InstanceId = $InstanceId"
Write-Verbose "InstanceName = $InstanceName"
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

if ($InstanceName -ne '')
{
	$instances = Get-CrmInstances -ApiUrl $ApiUrl -Credential $Cred

	Foreach($instance in $instances)
	{
		if ($instance.DomainName -ieq $InstanceName)
		{
			$foundId = $instance.Id
		}
	}

	if ($foundId -eq $null)
	{
		throw "$InstanceName not found"
	}
}

if ($foundId -ne $null)
{
	$InstanceId = $foundId
}

$operation = Remove-CrmInstance -ApiUrl $ApiUrl -Id $instanceId -Credential $Cred

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

Write-Verbose 'Leaving DeleteOnlineInstance.ps1'

