#
# PackCmData.ps1
#

[CmdletBinding()]

param(
[string]$dataFile, #The absolute path of data.xml to create/update
[string]$extractFolder, #The absoluate path to folder for extracting the data zip file
[string]$combineDataXmlFileLevel
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering PackCmData.ps1'

#Print Parameters

Write-Verbose "dataFile = $dataFile"
Write-Verbose "combineDataXmlFileLevel = $combineDataXmlFileLevel"
Write-Verbose "extractFolder = $extractFolder"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Compress-XrmCMData -DataZip "$dataFile" -Folder "$extractFolder" -CombineDataXmlFileLevel $combineDataXmlFileLevel

Write-Verbose 'Leaving PackCmData.ps1'
