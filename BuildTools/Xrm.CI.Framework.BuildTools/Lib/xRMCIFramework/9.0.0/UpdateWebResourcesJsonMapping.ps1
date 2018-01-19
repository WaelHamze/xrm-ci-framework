#
# UpdateWebResourcesJsonMapping.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$WebResourceFolderPath,
	[string]$WebResourceJsonMappingPath,
	[bool]$Publish, #Will publish the web resource
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateWebResourcesJsonMapping.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "WebResourceFolderPath = $WebResourceFolderPath"
Write-Verbose "WebResourceJsonMappingPath = $WebResourceJsonMappingPath"
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
$json = Get-Content -Raw -Path $WebResourceJsonMappingPath | ConvertFrom-Json
$json.webresources | ForEach-Object {
    $_.files | ForEach-Object {
	    $WebResourcePath = [System.Uri]::UnescapeDataString([System.IO.Path]::Combine($WebResourceFolderPath, $_.file))
	    Write-Verbose "Updating Web Resource: $WebResourcePath"          
	    Set-XrmWebResource -Path $WebResourcePath -UniqueName $_.uniquename -Publish $Publish -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose
	    Write-Verbose "Updated Web Resource"
    }
}

Write-Verbose 'Leaving UpdateWebResourcesJsonMapping.ps1' -Verbose
