#
# ManageXrmConnections.ps1
#
param(
[string]$key
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ManageXrmConnections.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

."$scriptPath\ConnectionFunctions.ps1"

$connection = GetXrmConnectionFromConfig($key);

Write-Verbose 'Leaving ManageXrmConnections.ps1'
