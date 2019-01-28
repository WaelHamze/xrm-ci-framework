#
# ExportSolutionsUsingConfig.ps1
#

param(
[string]$CrmConnectionString,
[string]$OutputFolder,
[string]$ConfigFilePath,
[int]$Timeout
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ExportSolutionsUsingConfig.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "OutputFolder = $OutputFolder"
Write-Verbose "ConfigFilePath = $ConfigFilePath"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

#Solution Export

Write-Host "Exporting Solutions"
        
$exportedFiles = Export-XrmSolutions -ConnectionString "$CrmConnectionString" -ConfigFilePath "$ConfigFilePath" -OutputFolder "$OutputFolder" -Timeout $Timeout
    
Write-Host "Managed Solution Exported $exportedFiles"

Write-Verbose 'Leaving ExportSolutionsUsingConfig.ps1'