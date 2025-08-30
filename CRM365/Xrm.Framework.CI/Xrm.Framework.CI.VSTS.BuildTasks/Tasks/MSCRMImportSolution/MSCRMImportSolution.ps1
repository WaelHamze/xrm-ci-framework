[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMImportSolution.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$solutionFile = Get-VstsInput -Name solutionFile -Require
$publishWorkflows = Get-VstsInput -Name publishWorkflows -AsBool
$overwriteUnmanagedCustomizations = Get-VstsInput -Name overwriteUnmanagedCustomizations -AsBool
$skipProductUpdateDependencies = Get-VstsInput -Name skipProductUpdateDependencies -AsBool
$convertToManaged = Get-VstsInput -Name convertToManaged -AsBool
$holdingSolution = Get-VstsInput -Name holdingSolution -AsBool
$override = Get-VstsInput -Name override -AsBool
$asyncWaitTimeout = Get-VstsInput -Name asyncWaitTimeout -Require -AsInt
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#TFS Release Parameters
$artifactsFolder = $env:AGENT_RELEASEDIRECTORY 

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "solutionFile = $solutionFile"
Write-Verbose "publishWorkflows = $publishWorkflows"
Write-Verbose "overwriteUnmanagedCustomizations = $overwriteUnmanagedCustomizations"
Write-Verbose "skipProductUpdateDependencies = $skipProductUpdateDependencies"
Write-Verbose "convertToManaged = $convertToManaged"
Write-Verbose "holdingSolution = $holdingSolution"
Write-Verbose "override = $override"
Write-Verbose "asyncWaitTimeout = $asyncWaitTimeout"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"
Write-Verbose "artifactsFolder = $artifactsFolder"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

$solutionFilename = $solutionFile.Substring($solutionFile.LastIndexOf("\") + 1)

$logFilename = $solutionFilename.replace(".zip", "_importlog_" + [System.DateTime]::Now.ToString("yyyy_MM_dd__HH_mm") + ".xml")

& "$scriptPath\ps_modules\xRMCIFramework\ImportSolution.ps1" -solutionFile "$solutionFile" -crmConnectionString "$CrmConnectionString" -override $override -publishWorkflows $publishWorkflows -overwriteUnmanagedCustomizations $overwriteUnmanagedCustomizations -skipProductUpdateDependencies $skipProductUpdateDependencies -ConvertToManaged $convertToManaged -HoldingSolution $holdingSolution -logsDirectory "$artifactsFolder" -logFileName "$logFilename" -AsyncWaitTimeout $AsyncWaitTimeout -Timeout $crmConnectionTimeout

if (Test-Path "$artifactsFolder\$logFilename")
{
	Write-Host "##vso[task.uploadfile]$artifactsFolder\$logFilename"
}

Write-Verbose 'Leaving MSCRMImportSolution.ps1'
