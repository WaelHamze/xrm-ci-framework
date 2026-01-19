#
# PluginPackageRegistration.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$RegistrationType,
	[string]$PackagePath,
	[string]$PublisherPrefix,
	[string]$MappingFile,
	[string]$SolutionName,
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering PluginPackageRegistration.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "RegistrationType = $RegistrationType"
Write-Verbose "PackagePath = $PackagePath"
Write-Verbose "MappingFile = $MappingFile"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "PublisherPrefix = $PublisherPrefix"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit"
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Host "Updating Plugin Package: $PackagePath"

Set-XrmPluginPackageRegistration -RegistrationType $RegistrationType -PackagePath $PackagePath -MappingFile $MappingFile -SolutionName $SolutionName -PublisherPrefix $PublisherPrefix -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Host "Updated Plugin Package, Assembly and Steps"

Write-Verbose 'Leaving PluginPackageRegistration.ps1' -Verbose