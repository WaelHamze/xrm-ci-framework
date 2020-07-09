#
# PackSolutionsUsingConfig.ps1
#

param(
[string]$CoreToolsPath,
[string]$OutputFolder,
[string]$ConfigFilePath,
[string]$LogsDirectory
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering PackSolutionsUsingConfig.ps1'

#Parameters
Write-Verbose "CoreToolsPath = $CoreToolsPath"
Write-Verbose "OutputFolder = $OutputFolder"
Write-Verbose "ConfigFilePath = $ConfigFilePath"
Write-Verbose "LogsDirectory = $LogsDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

$SolutionPackagerPath = $CoreToolsPath + "\SolutionPackager.exe"

#Solution Pack

Write-Host "Packing Solutions"
        
$results = Compress-XrmSolutions -solutionPackagerPath "$SolutionPackagerPath" -ConfigFilePath "$ConfigFilePath" -OutputFolder "$OutputFolder" -LogsDirectory $LogsDirectory
    
Write-Host "Packed Solutions to $OutputFolder"

Write-Verbose 'Leaving PackSolutionsUsingConfig.ps1'