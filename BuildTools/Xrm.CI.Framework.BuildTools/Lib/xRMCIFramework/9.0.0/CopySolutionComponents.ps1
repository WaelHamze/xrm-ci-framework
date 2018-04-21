#
# Filename: ExtractSolution.ps1
#
param(
[string]$FromSolutionName, #The unique CRM solution name
[string]$ToSolutionName, #The unique CRM solution name
[string]$CrmConnectionString, #The connection string as per CRM Sdk
[int]$Timeout=360
) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MoveSolutionComponent.ps1'

Write-Verbose "FromSolutionName = $FromSolutionName"
Write-Verbose "ToSolutionName = $ToSolutionName"
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

Copy-XrmSolutionComponents -FromSolutionName $FromSolutionName -ToSolutionName $ToSolutionName -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Verbose 'Leaving MoveSolutionComponent.ps1'