#
# UpdateWebResource.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$WebResourceProjectPath,
	[string]$Publish, #Will publish the web resource
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateWebResource.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "WebResourceProjectPath = $WebResourceProjectPath"
Write-Verbose "Publish = $Publish"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Host "Updating Web Resource: $WebResourceProjectPath"

Set-XrmWebResource -WebResourceProjectPath $WebResourceProjectPath -Publish $Publish -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Host "Updated Web Resource"

Write-Verbose 'Leaving UpdateWebResource.ps1' -Verbose
