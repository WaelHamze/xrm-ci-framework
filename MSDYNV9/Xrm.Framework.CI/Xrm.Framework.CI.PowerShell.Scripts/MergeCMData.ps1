#
# MergeCmData.ps1
#

[CmdletBinding()]

param(
[string]$mergeFolder, #The absolute path to folder for merging changes into
[string]$mappingFile, #The absolute path of the mapping file containing merge instructions
[string]$mergeDataLevel, #Set to Default, None, EntityLevel, RecordLevel to specify level of target folder split 
[bool]$fileMapCaseSensitive #Set to true to enforce case sensitivity of mapped file names
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering MergeCmData.ps1'

#Print Parameters

Write-Verbose "mappingFile = $mappingFile"
Write-Verbose "mergeFolder = $mergeFolder"
Write-Verbose "mergeDataLevel = $mergeDataLevel"
Write-Verbose "fileMapCaseSensitive = $fileMapCaseSensitive"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Merge-XrmCmData -Folder "$mergeFolder" -MappingFile $mappingFile -MergeDataXmlFileLevel $mergeDataLevel -FileMapCaseSensitive $fileMapCaseSensitive

Write-Verbose 'Leaving MergeCmData.ps1'
