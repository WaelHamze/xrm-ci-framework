#
# CopyOnlineInstance.ps1
#

param(
[string]$ApiUrl,
[string]$Username,
[string]$Password,
[string]$SourceInstanceName,
[string]$TargetInstanceName,
[string]$CopyType,
[string]$FriendlyName,
[string]$SecurityGroupName,
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
Write-Verbose "TargetInstanceName = $TargetInstanceName"
Write-Verbose "CopyType = $CopyType"
Write-Verbose "FriendlyName = $FriendlyName"
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

$sourceInstance = Get-XrmInstanceByName -ApiUrl $ApiUrl -Cred $Cred -InstanceName $SourceInstanceName

if ($sourceInstance)
{
    Write-Host "Source Instance Id $($sourceInstance.Id)"
}
else
{
    throw "$SourceInstanceName not found"
}

$targetInstance = Get-XrmInstanceByName -ApiUrl $ApiUrl -Cred $Cred -InstanceName $TargetInstanceName

if ($targetInstance)
{
    Write-Host "Target Instance Id $($targetInstance.Id)"
}
else
{
    throw "$TargetInstanceName not found"
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

if ($FriendlyName)
{
    Write-Verbose "Using supplied friendly name $FriendlyName"
}
else
{
    Write-Verbose "Setting friendly name to $($targetInstance.FriendlyName)"   
    $FriendlyName = $targetInstance.FriendlyName
}

$CopyParams = @{
	TargetInstanceId = $targetInstance.Id
	CopyType = $CopyType
	FriendlyName = $FriendlyName
}

if ($SecurityGroupId)
{
	$CopyParams.SecurityGroupId = $SecurityGroupId
}

$copyInfo = New-CrmInstanceCopyRequestInfo @CopyParams


$operation = Copy-CrmInstance -ApiUrl $ApiUrl -Credential $Cred -CopyInstanceRequestDetails $copyInfo -SourceInstanceIdToCopy $sourceInstance.Id

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

Write-Verbose 'Leaving CopyOnlineInstance.ps1'