[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMSetVersion.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$solutionName = Get-VstsInput -Name solutionName -Require
$versionNumber = Get-VstsInput -Name versionNumber -Require

#TFS Build Parameters
$buildNumber = $env:BUILD_BUILDNUMBER
$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY
$binariesDirectory = $env:BUILD_BINARIESDIRECTORY

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "versionNumber = $versionNumber"

Write-Verbose "buildNumber = $buildNumber"
Write-Verbose "sourcesDirectory = $sourcesDirectory"
Write-Verbose "binariesDirectory = $binariesDirectory"

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\UpdateSolutionVersionInCRM.ps1" -CrmConnectionString $crmConnectionString -SolutionName $solutionName -VersionNumber $versionNumber

Write-Verbose 'Leaving MSCRMSetVersion.ps1'
