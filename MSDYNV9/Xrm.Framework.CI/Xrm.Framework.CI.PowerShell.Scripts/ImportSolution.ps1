#
# Filename: ImportSolution.ps1
#
[CmdletBinding()]

param(
[string]$solutionFile, #The absolute path to the solution file zip to be imported
[string]$crmConnectionString, #The target CRM organization connection string
[bool]$override, #If set to 1 will override the solution even if a solution with same version exists
[bool]$publishWorkflows, #Will publish workflows during import
[bool]$overwriteUnmanagedCustomizations, #Will overwrite unmanaged customizations
[bool]$skipProductUpdateDependencies, #Will skip product update dependencies
[bool]$convertToManaged, #Direct the system to convert any matching unmanaged customizations into your managed solution. Optional.
[bool]$holdingSolution, #Imports by creating a holding/upgrade solution
[bool]$ImportAsync = $true, #Import solution in Async Mode, recommended
[int]$AsyncWaitTimeout, #Optional - Async wait timeout in seconds
[int]$Timeout, #Optional - CRM connection timeout
[string]$logsDirectory, #Optional - will place the import log in here
[string]$logFilename #Optional - will use this as import log file name
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

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

$importJobId = [guid]::NewGuid()

Import-XrmSolution -ConnectionString "$CrmConnectionString" -SolutionFilePath "$solutionFile" -publishWorkflows $publishWorkflows -overwriteUnmanagedCustomizations $overwriteUnmanagedCustomizations -SkipProductUpdateDependencies $skipProductUpdateDependencies -ConvertToManaged $convertToManaged -HoldingSolution $holdingSolution -OverrideSameVersion $override -ImportAsync $ImportAsync -ImportJobId $importJobId -AsyncWaitTimeout $AsyncWaitTimeout -DownloadFormattedLog $true -LogsDirectory "$logsDirectory" -LogFileName "$logFilename" -Timeout $Timeout
 
Write-Verbose 'Leaving ImportSolution.ps1'