#
# SplitPluginAssembly.ps1
#

param(
	[string]$regexType,
	[string]$regex,
	[string]$projectFilePath,
	[string]$solutionFilePath
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering SplitPluginAssembly.ps1' -Verbose

#Parameters
Write-Verbose "regexType = $regexType"
Write-Verbose "regex = $regex"
Write-Verbose "projectFilePath = $projectFilePath"
Write-Verbose "solutionFilePath = $solutionFilePath"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Host "Split Plugin Assembly Started"

Split-XrmPluginAssembly -regexType $regexType -regex $regex -projectFilePath $projectFilePath -solutionFilePath $solutionFilePath

Write-Host "Split Plugin Assembly Completed"

Write-Verbose 'Leaving SplitPluginAssembly.ps1' -Verbose
