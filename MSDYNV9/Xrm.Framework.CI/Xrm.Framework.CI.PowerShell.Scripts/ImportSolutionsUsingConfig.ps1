#
# ImportSolutionsUsingConfig.ps1
#

param(
[string]$CrmConnectionString,
[string]$LogsDirectory,
[string]$ConfigFilePath,
[int]$Timeout
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ImportSolutionsUsingConfig.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "LogsDirectory = $LogsDirectory"
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

#Solutions Import

Write-Verbose "Importing Solutions"
        
$importResults = Import-XrmSolutions -ConnectionString "$CrmConnectionString" -ConfigFilePath "$ConfigFilePath" -LogsDirectory "$LogsDirectory" -Timeout $Timeout

Write-Verbose 'Leaving ImportSolutionsUsingConfig.ps1'