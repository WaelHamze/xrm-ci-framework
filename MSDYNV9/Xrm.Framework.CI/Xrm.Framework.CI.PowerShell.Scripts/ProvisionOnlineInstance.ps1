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
[string]$PSModulePath,
[string]$AzureADModulePath
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

if ($SecurityGroupId)
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

if ($WaitForCompletion -and ($operation.OperationId -ne [system.guid]::empty)  -and ($OperationStatus -ne "Succeeded"))
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

#Removed code below as sometimes instances get created with numbers appended to name like [instancename]0
if ($WaitForCompletion -and $false)
{	
	#Sometimes instance is created but the API still returns NOT FOUND (no other instances available).
	#Added this delay to give chance for operation to progress.

	Write-Host "Starting Initial Sleep for 30 seconds"

	Start-Sleep -Seconds 30

	Write-Host "Attempting to poll for instance every $SleepDuration secs"
	
	$provisioning = $true
	while ($provisioning)
	{
		Write-Verbose "Starting Sleep for $SleepDuration seconds"
		
		Start-Sleep -Seconds $SleepDuration

		Write-Verbose "Retrieving instance"

		$instance = Get-XrmInstanceByName -ApiUrl $ApiUrl -Cred $Cred -InstanceName $DomainName

		if ($instance)
		{
			$State = $instance.State
			
			Write-Host "Instance State: $State"

			if ($State -eq "Ready")
			{
				$provisioning = $false
			}
		}
		else
		{
			Write-Host "$DomainName not found"
		}
	}
}

Write-Verbose 'Leaving ProvisionOnlineInstance.ps1'
