#
# Filename: ImportSolution.ps1
#
param(
[string]$solutionFile, #The absolute path to the solution file zip to be imported
[string]$crmConnectionString, #The target CRM organization connection string
[bool]$override, #If set to 1 will override the solution even if a solution with same version exists
[bool]$publishWorkflows, #Will publish workflows during import
[bool]$overwriteUnmanagedCustomizations, #Will overwrite unmanaged customizations
[bool]$skipProductUpdateDependencies, #Will skip product update dependencies
[bool]$convertToManaged, #Direct the system to convert any matching unmanaged customizations into your managed solution. Optional.
[bool]$holdingSolution,
[bool]$ImportAsync = $true,
[int]$AsyncWaitTimeout, #Optional - Async wait timeout in seconds
[int]$Timeout, #Optional - CRM connection timeout
[string]$logsDirectory, #Optional - will place the import log in here
[string]$logFilename #Optional - will use this as import log file name
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ImportSolution.ps1'

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Verbose "solutionFile = $solutionFile"
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "override = $override"
Write-Verbose "publishWorkflows = $publishWorkflows"
Write-Verbose "overwriteUnmanagedCustomizations = $overwriteUnmanagedCustomizations"
Write-Verbose "skipProductUpdateDependencies = $skipProductUpdateDependencies"
Write-Verbose "convertToManaged = $convertToManaged"
Write-Verbose "holdingSolution = $holdingSolution"
Write-Verbose "AsyncWaitTimeout = $AsyncWaitTimeout"
Write-Verbose "ImportAsync = $ImportAsync"
Write-Verbose "AsyncWaitTimeout = $AsyncWaitTimeout"
Write-Verbose "Timeout = $Timeout"
Write-Verbose "logsDirectory = $logsDirectory"
Write-Verbose "logFilename = $logFilename"

Write-Verbose "Getting solution info from zip"

$solutionInfo = Get-XrmSolutionInfoFromZip -SolutionFilePath "$solutionFile"

Write-Host "Solution Name: " $solutionInfo.UniqueName
Write-Host "Solution Version: " $solutionInfo.Version

$solution = Get-XrmSolution -ConnectionString "$CrmConnectionString" -UniqueSolutionName $solutionInfo.UniqueName

if ($solution -eq $null)
{
    Write-Host "Solution not currently installed"
}
else
{
    Write-Host "Solution Installed Current version: " $solution.Version
}
 
if ($override -or ($solution -eq $null) -or ($solution.Version -ne $solutionInfo.Version))
{    
    Write-Verbose "Importing Solution: $solutionFile"

    $importJobId = [guid]::NewGuid()

	Write-Host "Solution Import Starting. Import Job Id: $importJobId"

    if ($logsDirectory)
    {
	}
	else
	{
		$logsDirectory = $scriptPath;
	}

	if ($logFilename)
	{
	}
	else
	{
		$logFilename = $solutionInfo.UniqueName + '_' + ($solutionInfo.Version).replace('.','_') + '_' + [System.DateTime]::Now.ToString("yyyy_MM_dd__HH_mm") + ".xml"
	}

	Import-XrmSolution -ConnectionString "$CrmConnectionString" -SolutionFilePath "$solutionFile" -publishWorkflows $publishWorkflows -overwriteUnmanagedCustomizations $overwriteUnmanagedCustomizations -SkipProductUpdateDependencies $skipProductUpdateDependencies -ConvertToManaged $convertToManaged -HoldingSolution $holdingSolution -ImportAsync $ImportAsync -ImportJobId $importJobId -AsyncWaitTimeout $AsyncWaitTimeout -DownloadFormattedLog $true -LogsDirectory "$logsDirectory" -LogFileName "$logFilename" -Timeout $Timeout -Verbose
 
    Write-Host "Solution Import Completed. Import Job Id: $importJobId"
}
else
{
    Write-Host "Skipped Import of Solution..."
}

Write-Verbose 'Leaving ImportSolution.ps1'