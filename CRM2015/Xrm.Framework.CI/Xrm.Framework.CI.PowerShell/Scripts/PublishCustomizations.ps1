param(
[string]$CrmConnectionString
)

$ErrorActionPreference = "Stop"

#Parameters
Write-Host "CrmConnectionString: $CrmConnectionString"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.dll"
Write-Host "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Host "Imported CIToolkit"


#Solution Publish Customizations

Write-Host "Publishing Customizations"

Publish-XrmCustomizations -ConnectionString $CrmConnectionString

Write-Host "Publishing Customizations Completed"