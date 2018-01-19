#
# UpdateDevloperToolkitWebResources.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$WebResourceProjectPath,
	[bool]$Publish, #Will publish the web resource
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateDevloperToolkitWebResources.ps1' -Verbose

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
$WebResourceProjectFolderPath = [System.IO.Path]::GetDirectoryName($WebResourceProjectPath)+"\";
[xml]$xml = Get-Content $WebResourceProjectPath
$xml.Project.ItemGroup.CRMWebResource | ForEach-Object {
    if($_.Include){
      $WebResourcePath = [System.Uri]::UnescapeDataString([System.IO.Path]::Combine($WebResourceProjectFolderPath, $_.Include))
	    Write-Verbose "Updating Web Resource: $WebResourcePath"          
	    Set-XrmWebResource -Path $WebResourcePath -UniqueName $_.UniqueName -Publish $Publish -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose
	    Write-Verbose "Updated Web Resource"
    }
}



Write-Verbose 'Leaving UpdateDevloperToolkitWebResources.ps1' -Verbose
