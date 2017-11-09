#
# PublishCustomizations.ps1
#

param(
[string]$CrmConnectionString,
[int]$Timeout = 120
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering PublishCustomizations.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"


#Solution Publish Customizations

Write-Host "Publishing Customizations"

Publish-XrmCustomizations -ConnectionString $CrmConnectionString -Timeout $Timeout

Write-Host "Publishing Customizations Completed"

Write-Verbose 'Leaving PublishCustomizations.ps1' -Verbose