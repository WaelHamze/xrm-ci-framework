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

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\CopySolutionComponents.ps1" -fromSolutionName "$fromSolutionName" -toSolutionName "$toSolutionName" -crmConnectionString "$CrmConnectionString" -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMCopySolutionComponents.ps1'
