#
# ExtractCMData.ps1
#

[CmdletBinding()]

param(
[string]$dataFile, #The absolute path of data.zip to extract
[string]$extractFolder, #The absolute path to folder for extracting the data zip file
[bool]$sortExtractedData, #Set to true to sort the data.xml by record ids
[bool]$splitExtractedData, #Set to true to split the data.xml into multiple files per entity
[string]$splitExtractedDataLevel #Set to Default, None, EntityLevel, RecordLevel to specify level of split 
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ExtractCMData.ps1'

#Print Parameters

Write-Verbose "dataFile = $dataFile"
Write-Verbose "sortExtractedData = $sortExtractedData"
Write-Verbose "splitExtractedData = $splitExtractedData"
Write-Verbose "extractFolder = $extractFolder"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Expand-XrmCMData -DataZip "$dataFile" -Folder "$extractFolder" -SplitDataXmlFile $splitExtractedData -SplitDataXmlFileLevel $splitExtractedDataLevel -SortDataXmlFile $sortExtractedData

Write-Verbose 'Leaving ExtractCMData.ps1'
