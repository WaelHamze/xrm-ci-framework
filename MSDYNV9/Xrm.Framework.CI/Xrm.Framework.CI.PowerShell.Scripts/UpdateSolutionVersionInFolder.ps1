#
# UpdateSolutionVersionInFolder.ps1
#

param(
[string]$unpackedFilesFolder,
[string]$VersionNumber
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateSolutionVersionInFolder.ps1'

#Parameters
Write-Verbose "unpackedFilesFolder = $unpackedFilesFolder"
Write-Verbose "VersionNumber = $VersionNumber"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Verbose "Setting Solution Version in File to: $VersionNumber"

$SolutionXmlFile = "$UnpackedFilesFolder\Other\Solution.xml"

Write-Verbose "Setting $SolutionXmlFile to IsReadyOnly = false"

Set-ItemProperty $SolutionXmlFile -name IsReadOnly -value $false

Set-XrmSolutionVersionInFolder -SolutionFilesFolderPath $UnpackedFilesFolder -Version $VersionNumber

Write-Host "$SolutionXmlFile updated with $VersionNumber"

Write-Verbose 'Leaving UpdateSolutionVersionInFolder.ps1'
