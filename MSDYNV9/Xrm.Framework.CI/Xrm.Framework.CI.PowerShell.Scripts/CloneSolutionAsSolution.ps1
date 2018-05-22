#
# Filename: CloneSolutionAsSolution.ps1
#
param(
[string]$DisplayName, #Solution display name
[string]$ParentSolutionUniqueName, #The unique name of parent CRM solution
[string]$VersionNumber, #The version number of solution and has to be greater than parent solution
[string]$CrmConnectionString, #The connection string as per CRM Sdk
[int]$Timeout=360
) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering CloneSolutionAsSolution.ps1'

Write-Verbose "DisplayName = $DisplayName"
Write-Verbose "ParentSolutionUniqueName = $ParentSolutionUniqueName"
Write-Verbose "VersionNumber = $VersionNumber"
Write-Verbose "ConnectionString = $CrmConnectionString"
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

Write-Verbose 'Leaving CloneSolutionAsSolution.ps1'