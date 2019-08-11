#
# Filename: ExtractSolution.ps1
#
param([string]$UnpackedFilesFolder, #The folder to extract the CRM solution
[string]$mappingFile, #The full path to the mapping file
[string]$PackageType, #Managed/Unmanaged/Both
[string]$solutionName, #The unique CRM solution name
[string]$connectionString, #The connection string as per CRM Sdk
[string]$solutionFile, #The path to the solution file to be extracted. If supplied export is skipped
[string]$CoreToolsPath, #The full path to the Coretools folder containg solutionpackager.exe
[string]$sourceLoc,
[bool]$localize,
[bool]$TreatUnpackWarningsAsErrors
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ExtractSolution.ps1'

Write-Verbose "UnpackedFilesFolder = $UnpackedFilesFolder"
Write-Verbose "MappingFile = $mappingFile"
Write-Verbose "PackageType = $PackageType"
Write-Verbose "SolutionName = $solutionName"
Write-Verbose "ConnectionString = $connectionString"
Write-Verbose "SolutionFile = $solutionFile"
Write-Verbose "CoreToolsPath = $CoreToolsPath"
Write-Verbose "SourceLoc = $sourceLoc"
Write-Verbose "Localize = $localize"
Write-Verbose "TreatUnpackWarningsAsErrors = $TreatUnpackWarningsAsErrors"

# CI Toolkit
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit

#Locate SolutionPackager.exe
$SolutionPackagerFile = $scriptPath + "\SolutionPackager.exe"
if ($CoreToolsPath)
{
	$SolutionPackagerFile = $CoreToolsPath + "\SolutionPackager.exe"
}

#Export Solutions
if ($solutionFile)
{
	Write-Verbose "Using provided solution file"
}
else
{	
	Write-Output "Exporting Solutions to: " $env:TEMP

	if ($PackageType -ne "Unmanaged")
	{
		$managedSolution = Export-XrmSolution -ConnectionString $connectionString -Managed $True -OutputFolder $env:TEMP -UniqueSolutionName $solutionName
		Write-Output "Exported Solution: $managedSolution"
		$solutionFile = "$env:TEMP\$managedSolution"
	}

	if ($PackageType -ne "Managed")
	{
		$unmanagedSolution = Export-XrmSolution -ConnectionString $connectionString -Managed $False -OutputFolder $env:TEMP -UniqueSolutionName $solutionName
		Write-Output "Exported Solution: $unmanagedSolution"
		$solutionFile = "$env:TEMP\$unmanagedSolution"
	}
}

$SolutionPackagerFile = $scriptPath + "\SolutionPackager.exe"
if ($CoreToolsPath)
{
	$SolutionPackagerFile = $CoreToolsPath + "\SolutionPackager.exe"
}

$PackParams = @{
	SolutionPackagerPath = $SolutionPackagerFile
	PackageType = $PackageType
	Folder = $UnpackedFilesFolder
	SolutionFile = $solutionFile
	TreatWarningsAsErrors = $TreatUnpackWarningsAsErrors
}

if ($MappingFile)
{
	$PackParams.MappingFile = $MappingFile
}
if ($LogsDirectory)
{
	$PackParams.LogsDirectory = $LogsDirectory
}
if ($sourceLoc)
{
	$PackParams.SourceLoc = $sourceLoc
}
if ($localize)
{
	$PackParams.Localize = $localize
}

Expand-XrmSolution @PackParams

Write-Verbose 'Leaving ExtractSolution.ps1'
