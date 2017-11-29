#
# WaitForCRMOperation.ps1
#
# WaitForCRMOperation.ps1
#

param(
[string]$OperationId,
[string]$PSModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering WaitForCRMOperation.ps1'

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

$completed = $false
Write-Verbose "Waiting for completion...$OperationId"

while ($completed -eq $false)
{	
	Start-Sleep -Seconds $SleepDuration
    
	$OpStatus = Get-CrmOperationStatus -ApiUrl $ApiUrl -Credential $Cred -Id $OperationId

	$OperationStatus = $OpStatus.Status
    
	Write-Verbose "Status = $OperationStatus"

	if ($OperationStatus -in "FailedToCreate", "Failed", "Cancelling", "Cancelled", "Aborting", "Aborted", "Tombstone", "Deleting", "Deleted")
	{
		throw "CRM Operation : $OperationStatus"
	}
    
	if ($OperationStatus -eq "Succeeded")
	{
		$completed = $true
	}
}

Write-Verbose 'Leaving WaitForCRMOperation.ps1'

# OperationStatus: https://docs.microsoft.com/en-us/rest/api/admin.services.crm.dynamics.com/getoperationstatus#definitions_operationstatus
# None
# FailedToCreate
# NotStarted
# Ready
# Pending
# Running
# Succeeded
# Failed
# Cancelling
# Cancelled
# Aborting
# Aborted
# Tombstone
# Deleting
# Deleted
