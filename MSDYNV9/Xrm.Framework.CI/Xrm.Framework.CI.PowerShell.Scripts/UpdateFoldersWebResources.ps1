#
# UpdateFoldersWebResources.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$WebResourceFolderPath,
	[string]$SearchPattern, 
	[string]$RegExToMatchUniqueName,
	[bool]$IncludeFileExtensionForUniqueName,
	[bool]$Publish, #Will publish the web resource
	[string]$SolutionName,
	[bool]$FailIfWebResourceNotFound, 
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateFoldersWebResources.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "WebResourceFolderPath = $WebResourceFolderPath"
Write-Verbose "SearchPattern = $SearchPattern" 
Write-Verbose "RegExToMatchUniqueName = $RegExToMatchUniqueName"
Write-Verbose "IncludeFileExtensionForUniqueName = $IncludeFileExtensionForUniqueName"
Write-Verbose "Publish = $Publish"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "FailIfWebResourceNotFound = $FailIfWebResourceNotFound" 
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"
Set-XrmWebResourcesFromFolder -Path $WebResourceFolderPath -SolutionName $SolutionName -SearchPattern $SearchPattern -RegExToMatchUniqueName $RegExToMatchUniqueName -IncludeFileExtensionForUniqueName $IncludeFileExtensionForUniqueName -FailIfWebResourceNotFound $FailIfWebResourceNotFound -Publish $Publish -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose 
Write-Verbose 'Leaving UpdateFoldersWebResources.ps1' -Verbose
