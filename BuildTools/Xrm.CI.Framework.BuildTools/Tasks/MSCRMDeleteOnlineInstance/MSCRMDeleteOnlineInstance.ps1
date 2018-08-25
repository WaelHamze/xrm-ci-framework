[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMDeleteOnlineInstance.ps1'

$apiUrl = Get-VstsInput -Name apiUrl -Require
$username = Get-VstsInput -Name username -Require
$password = Get-VstsInput -Name password -Require
$instanceName = Get-VstsInput -Name instanceName -Require
$waitForCompletion = Get-VstsInput -Name waitForCompletion -AsBool
$sleepDuration = Get-VstsInput -Name sleepDuration -AsInt

#Print Verbose
Write-Verbose "apiUrl = $apiUrl"
Write-Verbose "username = $username"
Write-Verbose "instanceName = $instanceName"
Write-Verbose "waitForCompletion = $waitForCompletion"
Write-Verbose "sleepDuration = $sleepDuration"

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

$PSModulePath = "$mscrmToolsPath\OnlineManagementAPI\1.0.0"

& "$mscrmToolsPath\xRMCIFramework\9.0.0\DeleteOnlineInstance.ps1" -ApiUrl $apiUrl -Username $username -Password $password  -InstanceName $InstanceName -PSModulePath $PSModulePath -WaitForCompletion $WaitForCompletion -SleepDuration $sleepDuration

Write-Verbose 'Leaving MSCRMDeleteOnlineInstance.ps1'
