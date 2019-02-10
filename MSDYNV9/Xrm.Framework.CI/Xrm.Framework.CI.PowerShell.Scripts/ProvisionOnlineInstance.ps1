#
# ProvisionOnlineInstance.ps1
#
[CmdletBinding()]
param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$DomainName,
[string]$FriendlyName,
[string]$Purpose,
[string]$InitialUserEmail,
[string]$InstanceType,
[string]$ReleaseId,
[string[]]$TemplateNames,
[string]$LanguageId,
[string]$CurrencyCode,
[string]$CurrencyName,
[int]$CurrencyPrecision,
[string]$CurrencySymbol,
[string]$SecurityGroupId,
[string]$SecurityGroupName,
[bool]$WaitForCompletion = $false,
[int]$SleepDuration = 3,
[string]$PSModulePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering BackupCRMOnlineInstance.ps1'

#Parameters
Write-Verbose "ApiUrl = $ApiUrl"
Write-Verbose "Username = $Username"
Write-Verbose "DomainName = $DomainName"
Write-Verbose "FriendlyName = $FriendlyName"
Write-Verbose "Purpose = $Purpose"
Write-Verbose "InitialUserEmail = $InitialUserEmail"
Write-Verbose "InstanceType = $InstanceType"
Write-Verbose "ReleaseId = $ReleaseId"
Write-Verbose "TemplateNames = $TemplateNames"
Write-Verbose "LanguageId = $LanguageId"
Write-Verbose "CurrencyCode = $CurrencyCode"
Write-Verbose "CurrencyName = $CurrencyName"
Write-Verbose "CurrencyPrecision = $CurrencyPrecision"
Write-Verbose "CurrencySymbol = $CurrencySymbol"
Write-Verbose "SecurityGroupId = $SecurityGroupId"
Write-Verbose "SecurityGroupName = $SecurityGroupName"
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
 
$InstanceInfoParams = @{
	BaseLanguage = $LanguageId
	DomainName = $DomainName
	FriendlyName = $FriendlyName
	InitialUserEmail = $InitialUserEmail
	InstanceType = $InstanceType
	ServiceVersionId = $ReleaseId
	Purpose = $Purpose
	TemplateList = $TemplateNames
}

if ($CurrencyCode -ne '')
{
	$InstanceInfoParams.CurrencyCode = $CurrencyCode
}
if ($CurrencyName -ne '')
{
	$InstanceInfoParams.CurrencyName = $CurrencyName
}
if ($CurrencyPrecision -ne 0)
{
	$InstanceInfoParams.CurrencyPrecision = $CurrencyPrecision
}
if ($CurrencySymbol -ne '')
{
	$InstanceInfoParams.CurrencySymbol = $CurrencySymbol
}

if ($SecurityGroupName -ne '')
{
	Write-Verbose "Importing Module AzureAD" 
	Import-Module AzureAD
	Write-Verbose "Imported Module AzureAD" 
	
	$group = Get-AzureADGroup -Filter "DisplayName eq '$SecurityGroupName'"

	if ($group -ne $null)
	{
		$SecurityGroupId = $group.ObjectId
	}
	if ($group -eq $null)
	{
		throw "$SecurityGroupName not found"
	}
}

if ($SecurityGroupId -ne '')
{
	$InstanceInfoParams.SecurityGroupId = $SecurityGroupId
}

$instanceInfo = New-CrmInstanceInfo @InstanceInfoParams

$operation = New-CrmInstance -ApiUrl $ApiUrl -NewInstanceInfo $instanceInfo -Credential $Cred

$OperationId = $operation.OperationId
$OperationStatus = $operation.Status

Write-Output "OperationId = $OperationId"
Write-Host "Status = $OperationStatus"

if ($operation.Errors.Count -gt 0)
{
    $errorMessage = $operation.Errors[0].Description
    throw "Errors encountered : $errorMessage"
}

if ($WaitForCompletion -and ($OperationStatus -ne "Succeeded"))
{
	Write-Verbose "Waiting for AsyncOperation to complete"

	$status = Wait-XrmOperation -ApiUrl $ApiUrl -Cred $Cred -operationId $operation.OperationId

	$status

	if ($status.Status -ne "Succeeded")
	{
		throw "Operation status: $status.Status"
	}
}
else
{
	Write-Verbose "Skipped waiting for Async Operation"
}

if ($WaitForCompletion)
{
	#Sometimes instance is created but the API still returns NOT FOUND (no other instances available).
	#Added this delay to give chance for operation to progress.

	Write-Verbose "Starting Initial Sleep for 30 seconds"

	Start-Sleep -Seconds 30
	
	$provisioning = $true
	while ($provisioning)
	{
		Write-Verbose "Starting Sleep for $SleepDuration seconds"
		
		Start-Sleep -Seconds $SleepDuration

		Write-Verbose "Retrieving instance"

		$instance = Get-XrmInstanceByName -ApiUrl $ApiUrl -Cred $Cred -InstanceName $DomainName

		$State = $instance.State

		Write-Verbose "Instance State: $State"

		if (($instance -ne $null) -and ($State -eq "Ready"))
		{
			$provisioning = $false
		}
	}
}

Write-Verbose 'Leaving ProvisionOnlineInstance.ps1'
