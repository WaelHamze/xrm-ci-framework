#
# Filename: CloneSolution.ps1
#
param(
[string]$CrmConnectionString, #The connection string as per CRM Sdk
[string]$ParentSolutionUniqueName, #The unique name of parent CRM solution
[string]$DisplayName, #Solution display name
[string]$VersionNumber, #The version number of solution and has to be greater than parent solution
[int]$Timeout=360
) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering CloneSolution.ps1'

Write-Verbose "ConnectionString = $CrmConnectionString"
Write-Verbose "ParentSolutionUniqueName = $ParentSolutionUniqueName"
Write-Verbose "DisplayName = $DisplayName"
Write-Verbose "VersionNumber = $VersionNumber"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Merge-XrmSolutionPatches -DisplayName $DisplayName -ParentSolutionUniqueName $ParentSolutionUniqueName -VersionNumber $VersionNumber -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Verbose 'Leaving CloneSolution.ps1'