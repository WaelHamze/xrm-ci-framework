#
# ExtractCmData.ps1
#

[CmdletBinding()]

param(
[string]$dataFile, #The absolute path of data.zip to extract
[string]$extractFolder, #The absolute path to folder for extracting the data zip file
[bool]$sortExtractedData, #Set to true to sort the data.xml by record ids
[string]$splitExtractedDataLevel #Set to Default, None, EntityLevel, RecordLevel to specify level of split 
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ExtractCmData.ps1'

#Print Parameters

Write-Verbose "dataFile = $dataFile"
Write-Verbose "sortExtractedData = $sortExtractedData"
Write-Verbose "splitExtractedDataLevel = $splitExtractedDataLevel"
Write-Verbose "extractFolder = $extractFolder"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Expand-XrmCmData -DataZip "$dataFile" -Folder "$extractFolder" -SplitDataXmlFileLevel $splitExtractedDataLevel -SortDataXmlFile $sortExtractedData

Write-Verbose 'Leaving ExtractCmData.ps1'
