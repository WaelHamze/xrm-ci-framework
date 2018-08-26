[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMSplitPluginAssembly.ps1'

#Get Parameters
$regexType = Get-VstsInput -Name regexType -Require
$regex = Get-VstsInput -Name regex -Require
$projectFilePath = Get-VstsInput -Name projectFilePath -Require
$solutionFilePath = Get-VstsInput -Name solutionFilePath 

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\SplitPluginAssembly.ps1" -regexType $regexType -regex $regex -projectFilePath $projectFilePath -solutionFilePath $solutionFilePath


Write-Verbose 'Leaving MSCRMSplitPluginAssembly.ps1'