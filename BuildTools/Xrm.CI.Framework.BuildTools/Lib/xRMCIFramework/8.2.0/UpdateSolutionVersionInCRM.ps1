#
# UpdateSolutionVersionInCRM.ps1
#

param(
[string]$CrmConnectionString,
[string]$SolutionName,
[string]$VersionNumber
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateSolutionVersionInCRM.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "VersionNumber = $VersionNumber"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Host "Updating Solution Version to $VersionNumber"

Set-XrmSolutionVersion -ConnectionString $CrmConnectionString -SolutionName $SolutionName -Version $VersionNumber

Write-Host "Solution Version Updated"

Write-Verbose 'Leaving UpdateSolutionVersionInCRM.ps1' -Verbose
