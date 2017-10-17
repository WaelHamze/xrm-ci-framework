[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMProvisionOnlineInstance.ps1'

#Get Parameters
$apiUrl = Get-VstsInput -Name apiUrl -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$domainName = Get-VstsInput -Name domainName -Require
$friendlyName = Get-VstsInput -Name friendlyName -Require
$purpose = Get-VstsInput -Name purpose
$initialUserEmail = Get-VstsInput -Name initialUserEmail -Require
$instanceType = Get-VstsInput -Name instanceType -Require
$serviceVersion = Get-VstsInput -Name serviceVersion -Require
$sales = Get-VstsInput -Name sales -AsBool
$customerService = Get-VstsInput -Name customerService -AsBool
$fieldService = Get-VstsInput -Name fieldService -AsBool
$projectService = Get-VstsInput -Name projectService -AsBool
$languageId = Get-VstsInput -Name languageId -AsInt
$currencyCode = Get-VstsInput -Name currencyCode
$currencyName = Get-VstsInput -Name currencyName
$currencyPrecision = Get-VstsInput -Name currencyPrecision -AsInt
$currencySymbol = Get-VstsInput -Name currencySymbol
$securityGroupId = Get-VstsInput -Name securityGroupId
$securityGroupName = Get-VstsInput -Name securityGroupName
$waitForCompletion = Get-VstsInput -Name waitForCompletion -AsBool
$sleepDuration = Get-VstsInput -Name sleepDuration -AsInt

#Print Verbose
Write-Verbose "apiUrl = $apiUrl"
Write-Verbose "username = $username"
Write-Verbose "domainName = $domainName"
Write-Verbose "friendlyName = $friendlyName"
Write-Verbose "purpose = $purpose"
Write-Verbose "initialUserEmail = $initialUserEmail"
Write-Verbose "instanceType = $instanceType"
Write-Verbose "serviceVersion = $serviceVersion"
Write-Verbose "customerService = $customerService"
Write-Verbose "fieldService = $fieldService"
Write-Verbose "projectService = $projectService"
Write-Verbose "languageId = $languageId"
Write-Verbose "currencyCode = $currencyCode"
Write-Verbose "currencyName = $currencyName"
Write-Verbose "currencyPrecision = $currencyPrecision"
Write-Verbose "currencySymbol = $currencySymbol"
Write-Verbose "securityGroupId = $securityGroupId"
Write-Verbose "securityGroupName = $securityGroupName"
Write-Verbose "waitForCompletion = $waitForCompletion"
Write-Verbose "sleepDuration = $sleepDuration"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

$PSModulePath = "$scriptPath\ps_modules\Microsoft.Xrm.OnlineManagementAPI"

if ($sales)
{
    $templatesNames = "D365_Sales"
}
if ($customerService)
{
    $templatesNames = $templatesNames += "D365_CustomerService"
}
if ($fieldService)
{
    $templatesNames = $templatesNames += "D365_FieldService"
}
if ($projectService)
{
    $templatesNames = $templatesNames += "D365_ProjectServiceAutomation"
}


& "$scriptPath\ProvisionOnlineInstance.ps1" -ApiUrl $apiUrl -Username $username -Password $password  -DomainName $domainName -FriendlyName $friendlyName -Purpose $purpose -InitialUserEmail $initialUserEmail -InstanceType $instanceType -ReleaseId $serviceVersion -TemplateNames $templateNames -LanguageId $languageId -CurrencyCode $currencyCode -CurrencyName $currencyName -CurrencyPrecision $currencyPrecision -CurrencySymbol $currencySymbol -PSModulePath $PSModulePath -WaitForCompletion $WaitForCompletion -SleepDuration $sleepDuration

Write-Verbose 'Leaving MSCRMProvisionOnlineInstance.ps1'
