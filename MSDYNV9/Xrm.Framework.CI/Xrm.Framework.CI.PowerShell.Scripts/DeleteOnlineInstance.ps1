#
# BackupCRMOnlineInstance.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
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
Write-Verbose "InstanceName = $InstanceName"
Write-Verbose "WaitForCompletion = $WaitForCompletion"
Write-Verbose "SleepDuration = $SleepDuration"
Write-Verbose "PSModulePath = $PSModulePath"

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

Write-Host "Deleting instance $InstanceName " + $instance.Id

$operation = Remove-CrmInstance -ApiUrl $ApiUrl -Id $instance.Id -Credential $Cred

$OperationId = $operation.OperationId
$OperationStatus = $operation.Status

Write-Output "OperationId = $OperationId"
Write-Host "Status = $OperationStatus"

if ($operation.Errors.Count -gt 0)
{
    $errorMessage = $operation.Errors[0].Description
    throw "Errors encountered : $errorMessage"
}

if ($WaitForCompletion -and ($operation.OperationId -ne [system.guid]::empty) -and ($OperationStatus -ne "Succeeded"))
{
	$status = Wait-XrmOperation -ApiUrl $ApiUrl -Cred $Cred -operationId $operation.OperationId

	$status

	if ($status.Status -ne "Succeeded")
	{
		throw "Operation status: $status.Status"
	}
}

Write-Host "Instance Deleted: $InstanceName " + $instance.Id

Write-Verbose 'Leaving DeleteOnlineInstance.ps1'

