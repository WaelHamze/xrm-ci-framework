#
# OnlineInstanceFunctions.ps1
#

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering OnlineInstanceFunctions.ps1'

function Get-XrmInstanceByName(
	[String]$ApiUrl,
	[Object]$Cred,
	[String]$InstanceName)
{
	$instances = Get-CrmInstances -ApiUrl $ApiUrl -Credential $Cred

	Foreach($instance in $instances)
	{
		if ($instance.DomainName -ieq $InstanceName)
		{
			Write-Output($instance)
			return
		}
	}
}

function Get-XrmBackupByLabel(
	[String]$ApiUrl,
	[Object]$Cred,
	[String]$InstanceId,
	[String]$Label)
{
	$instanceBackups = Get-CrmInstanceBackups -ApiUrl $ApiUrl -Credential $Cred -InstanceId $InstanceId

	Foreach($instanceBackup in $instanceBackups)
	{
		if ($instanceBackup.Label -ieq $Label)
		{
			Write-Output($instanceBackup)
			return
		}
	}
}

function Wait-XrmOperation(
	[String]$ApiUrl,
	[Object]$Cred,
	[String]$OperationId,
	[Int]$SleepDuration = 3
)
{
	$completed = $false
	Write-Verbose "Waiting for completion...$OperationId"

	while ($completed -eq $false)
	{	
		Start-Sleep -Seconds $SleepDuration
    
		$OpStatus = Get-CrmOperationStatus -ApiUrl $ApiUrl -Credential $Cred -Id $OperationId

		$OperationStatus = $OpStatus.Status
    
		Write-Verbose "Status = $OperationStatus"

		if ($OperationStatus -notin "None", "NotStarted", "Ready", "Pending", "Running", "Deleting", "Aborting", "Cancelling")
		{
            Write-Output($OpStatus)
			$completed = $true
			return
		}
	}
}

Write-Verbose 'Leaving OnlineInstanceFunctions.ps1'