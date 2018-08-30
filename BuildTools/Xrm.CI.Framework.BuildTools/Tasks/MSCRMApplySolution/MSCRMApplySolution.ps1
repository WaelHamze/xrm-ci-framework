[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMApplySolution.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$solutionName = Get-VstsInput -Name solutionName -Require
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\ApplySolution.ps1" -solutionName "$solutionName" -crmConnectionString "$CrmConnectionString" -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMApplySolution.ps1'
