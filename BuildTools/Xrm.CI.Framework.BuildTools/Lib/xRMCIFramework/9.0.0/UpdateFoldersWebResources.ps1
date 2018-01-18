#
# UpdateFoldersWebResources.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$WebResourceFolderPath,
	[string]$CommaSeparatedWebResourceExtensions,
	[string]$RegExToMatchUniqueName,
	[bool]$IncludeFileExtensionForUniqueName,
	[bool]$Publish, #Will publish the web resource
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateFoldersWebResources.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "WebResourceFolderPath = $WebResourceFolderPath"
Write-Verbose "CommaSeparatedWebResourceExtensions = $CommaSeparatedWebResourceExtensions"
Write-Verbose "RegExToMatchUniqueName = $RegExToMatchUniqueName"
Write-Verbose "IncludeFileExtensionForUniqueName = $IncludeFileExtensionForUniqueName"
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
[string]$RegEx = ''
$fileNames = Get-ChildItem -Path $WebResourceFolderPath -File -Include $CommaSeparatedWebResourceExtensions.Split(',') -Recurse | ForEach-Object {
	$WebResourcePath = $_.FullName
	Write-Verbose "Updating Web Resource: $WebResourcePath"
	if($RegExToMatchUniqueName){
		[string]$fileName = [System.IO.Path]::GetFileNameWithoutExtension($WebResourcePath)
		if($IncludeFileExtensionForUniqueName){		
			[string]$fileExtension = [System.IO.Path]::GetExtension($WebResourcePath)
			$RegEx = $RegExToMatchUniqueName + $fileExtension.Replace(".", "[.]")
		}

		$RegEx = $RegEx.Replace('$fileName',$fileName)
	}
	Set-XrmWebResource -Path $WebResourcePath -RegExToMatchUniqueName $RegEx -Publish $Publish -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose
	Write-Verbose "Updated Web Resource"
} 

Write-Verbose 'Leaving UpdateFoldersWebResources.ps1' -Verbose
