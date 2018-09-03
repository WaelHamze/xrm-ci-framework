[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMUpdateWebResources.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$webResourceDeploymentType = Get-VstsInput -Name webResourceDeploymentType -Require
$webResourceProjectPath = Get-VstsInput -Name webResourceProjectPath
$webResourceFolderPath = Get-VstsInput -Name webResourceFolderPath
$webResourceJsonMappingPath = Get-VstsInput -Name webResourceJsonMappingPath
$searchPattern = Get-VstsInput -Name searchPattern 
$regExToMatchUniqueName = Get-VstsInput -Name regExToMatchUniqueName
$includeFileExtensionForUniqueName = Get-VstsInput -Name includeFileExtensionForUniqueName -AsBool
$publish = Get-VstsInput -Name publish -AsBool
$solutionName = Get-VstsInput -Name solutionName
$failIfWebResourceNotFound = Get-VstsInput -Name failIfWebResourceNotFound -AsBool 
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "CrmConnectionString = $crmConnectionString"
Write-Verbose "WebResourceDepymentType = $webResourceDeploymentType"
Write-Verbose "WebResourceProjectPath = $webResourceProjectPath"
Write-Verbose "WebResourceFolderPath = $webResourceFolderPath"
Write-Verbose "WebResourceJsonMappingPath = $webResourceJsonMappingPath"
Write-Verbose "SearchPattern = $searchPattern" 
Write-Verbose "RegExToMatchUniqueName = $regExToMatchUniqueName"
Write-Verbose "IncludeFileExtensionForUniqueName = $includeFileExtensionForUniqueName"
Write-Verbose "Publish = $publish"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "FailIfWebResourceNotFound = $failIfWebResourceNotFound" 
Write-Verbose "CrmConnectionTimeout = $crmConnectionTimeout"

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

if ($webResourceDeploymentType -eq "developerToolkit"){
	& "$mscrmToolsPath\xRMCIFramework\9.0.0\UpdateDevloperToolkitWebResources.ps1" -CrmConnectionString $crmConnectionString -WebResourceProjectPath $webResourceProjectPath -Publish $publish -Timeout $crmConnectionTimeout
} elseif ($webResourceDeploymentType -eq "folderPath"){
	& "$mscrmToolsPath\xRMCIFramework\9.0.0\UpdateFoldersWebResources.ps1" -CrmConnectionString $crmConnectionString -WebResourceFolderPath $webResourceFolderPath -SearchPattern $searchPattern -RegExToMatchUniqueName $regExToMatchUniqueName -IncludeFileExtensionForUniqueName $includeFileExtensionForUniqueName -Publish $publish -FailIfWebResourceNotFound $failIfWebResourceNotFound -SolutionName $solutionName -Timeout $crmConnectionTimeout
} elseif ($webResourceDeploymentType -eq "jsonMapping"){
	& "$mscrmToolsPath\xRMCIFramework\9.0.0\UpdateWebResourcesJsonMapping.ps1" -CrmConnectionString $crmConnectionString -WebResourceFolderPath $webResourceFolderPath -WebResourceJsonMappingPath $webResourceJsonMappingPath -Publish $publish -Timeout $crmConnectionTimeout
}
Write-Verbose 'Leaving MSCRMUpdateWebResources.ps1'