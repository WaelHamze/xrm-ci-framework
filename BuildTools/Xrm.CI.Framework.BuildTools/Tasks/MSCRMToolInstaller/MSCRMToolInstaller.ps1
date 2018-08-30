[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMToolInstaller.ps1'

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"
Write-Verbose "env VSTS_TOOLS_PATH: $env:VSTS_TOOLS_PATH"
Write-Verbose "env AGENT_WORKFOLDER: $env:AGENT_WORKFOLDER"
if ($env:VSTS_TOOLS_PATH)
{
	$toolPath = $env:VSTS_TOOLS_PATH
}
else
{
	$toolPath = $env:AGENT_WORKFOLDER + "\tools"
}

Write-Host "Using Tools Path: $toolPath"

$frameworkCache = $toolPath + "\MSCRMBuildTools"
Write-Verbose "Framework Cache: $frameworkCache"

$currentVersion = '9.0.1'
$currentVersionPath = "$frameworkCache\$currentVersion"

if (Test-Path $frameworkCache)
{
	Write-Host "$frameworkCache already created"
}
else
{
	New-Item "$frameworkCache" -ItemType directory
}

if (Test-Path $currentVersionPath)
{
	Write-Host "$currentVersion already cached" | Out-Null
}
else
{
	New-Item "$currentVersionPath" -ItemType directory | Out-Null
	Copy-Item -Path "$scriptPath\Lib\**" -Destination $currentVersionPath -Force -Recurse

	Write-Host "Copy completed"
}

Write-Host "##vso[task.setvariable variable=MSCRM_Tools_Path]$currentVersionPath"

Write-Verbose 'Leaving MSCRMToolInstaller.ps1'