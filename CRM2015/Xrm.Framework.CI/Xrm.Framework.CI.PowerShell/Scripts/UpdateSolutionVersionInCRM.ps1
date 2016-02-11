param(
[string]$CrmConnectionString,
[string]$SolutionName,
[string]$VersionNumber
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "CrmConnectionString: $CrmConnectionString"
Write-Host "SolutionName: $SolutionName"
Write-Host "VersionNumber: $VersionNumber"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.dll"
Write-Host "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Host "Imported CIToolkit"

Write-Host "Updating Solution Version to $VersionNumber"

Set-XrmSolutionVersion -ConnectionString $CrmConnectionString -SolutionName $SolutionName -Version $VersionNumber

Write-Host "Solution Version Updated"
