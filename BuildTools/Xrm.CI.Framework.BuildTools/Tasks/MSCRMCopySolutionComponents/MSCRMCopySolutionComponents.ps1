[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMCopySolutionComponents.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$fromSolutionName = Get-VstsInput -Name fromSolutionName -Require
$toSolutionName = Get-VstsInput -Name toSolutionName -Require
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "fromSolutionName = $fromSolutionName"
Write-Verbose "toSolutionName = $toSolutionName"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\Lib\xRMCIFramework\9.0.0\CopySolutionComponents.ps1" -fromSolutionName "$fromSolutionName" -toSolutionName "$toSolutionName" -crmConnectionString "$CrmConnectionString" -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMCopySolutionComponents.ps1'
