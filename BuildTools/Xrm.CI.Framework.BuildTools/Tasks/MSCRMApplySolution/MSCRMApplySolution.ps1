[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMApplySolution.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$solutionName = Get-VstsInput -Name solutionName -Require
$asyncWaitTimeout = Get-VstsInput -Name asyncWaitTimeout -Require -AsInt
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "asyncWaitTimeout = $asyncWaitTimeout"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\Lib\xRMCIFramework\9.0.0\ApplySolution.ps1" -solutionName "$solutionName" -crmConnectionString "$CrmConnectionString" -AsyncWaitTimeout $AsyncWaitTimeout -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMApplySolution.ps1'
