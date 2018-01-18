[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMUpdateWebResources.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$webResourceDeploymentType = Get-VstsInput -Name webResourceDeploymentType -Require
$webResourceProjectPath = Get-VstsInput -Name webResourceProjectPath
$webResourceFolderPath = Get-VstsInput -Name webResourceFolderPath
$commaSeparatedWebResourceExtensions = Get-VstsInput -Name commaSeparatedWebResourceExtensions
$regExToMatchUniqueName = Get-VstsInput -Name regExToMatchUniqueName
$includeFileExtensionForUniqueName = Get-VstsInput -Name includeFileExtensionForUniqueName -AsBool
$publish = Get-VstsInput -Name publish -AsBool
$timeout = Get-VstsInput -Name timeout -Require

#Print Verbose
Write-Verbose "CrmConnectionString = $crmConnectionString"
Write-Verbose "WebResourceDepymentType = $webResourceDeploymentType"
Write-Verbose "WebResourceProjectPath = $webResourceProjectPath"
Write-Verbose "WebResourceFolderPath = $webResourceFolderPath"
Write-Verbose "CommaSeparatedWebResourceExtensions = $commaSeparatedWebResourceExtensions"
Write-Verbose "RegExToMatchUniqueName = $regExToMatchUniqueName"
Write-Verbose "IncludeFileExtensionForUniqueName = $includeFileExtensionForUniqueName"
Write-Verbose "Publish = $publish"
Write-Verbose "Timeout = $timeout"
#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

if ($webResourceDeploymentType -eq "developerToolkit"){
	& "$scriptPath\Lib\xRMCIFramework\9.0.0\UpdateDevloperToolkitWebResources.ps1" -CrmConnectionString $crmConnectionString -WebResourceProjectPath $webResourceProjectPath -Publish $publish -Timeout $timeout
} elseif ($webResourceDeploymentType -eq "folderPath"){
	& "$scriptPath\Lib\xRMCIFramework\9.0.0\UpdateFoldersWebResources.ps1" -CrmConnectionString $crmConnectionString -WebResourceFolderPath $webResourceFolderPath -CommaSeparatedWebResourceExtensions $commaSeparatedWebResourceExtensions -RegExToMatchUniqueName $regExToMatchUniqueName -IncludeFileExtensionForUniqueName $includeFileExtensionForUniqueName -Publish $publish -Timeout $timeout
} 
Write-Verbose 'Leaving MSCRMUpdateWebResources.ps1'