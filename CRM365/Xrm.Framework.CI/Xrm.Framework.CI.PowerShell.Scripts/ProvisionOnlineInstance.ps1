#
# ProvisionOnlineInstance.ps1
#

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

$instanceCurrency = New-CrmInstanceInfo -CurrencyCode $CurrencyCode -CurrencyName "$CurrencyName" -CurrencyPrecision $CurrencyPrecision -CurrencySymbol $CurrencySymbol

#Create Credentials
$SecPassword = ConvertTo-SecureString $Password -AsPlainText -Force
$Cred = New-Object System.Management.Automation.PSCredential ($Username, $SecPassword)
 
$instanceInfo = New-CrmInstanceInfo -BaseLanguage $LanguageId -DomainName $DomainName -FriendlyName $FriendlyName -InitialUserEmail $InitialUserEmail -InstanceType $InstanceType -ServiceVersionId $ReleaseId -Purpose $Purpose -TemplateList $TemplateNames

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
	& "$scriptPath\WaitForCRMOperation.ps1" -OperationId $OperationId -PSModulePath $PSModulePath
}

Write-Verbose 'Leaving ProvisionOnlineInstance.ps1'

