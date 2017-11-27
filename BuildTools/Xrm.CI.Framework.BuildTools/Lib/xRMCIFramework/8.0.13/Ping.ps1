#
# Ping.ps1
#
param(
[string]$CrmConnectionString
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering Ping.ps1'

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

#WhoAmI Check
$executingUser = Select-WhoAmI -ConnectionString $CrmConnectionString -Verbose

Write-Host "Ping Succeeded userId: " $executingUser.UserId

Write-Verbose 'Leaving Ping.ps1'
