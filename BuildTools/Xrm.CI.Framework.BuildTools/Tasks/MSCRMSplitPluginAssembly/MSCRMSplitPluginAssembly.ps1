[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMSplitPluginAssembly.ps1'

#Get Parameters
$regexType = Get-VstsInput -Name regexType -Require
$regex = Get-VstsInput -Name regex -Require
$projectFilePath = Get-VstsInput -Name projectFilePath -Require
$solutionFilePath = Get-VstsInput -Name solutionFilePath 

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

& "$scriptPath\Lib\xRMCIFramework\9.0.0\SplitPluginAssembly.ps1" -regexType $regexType -regex $regex -projectFilePath $projectFilePath -solutionFilePath $solutionFilePath


Write-Verbose 'Leaving MSCRMSplitPluginAssembly.ps1'