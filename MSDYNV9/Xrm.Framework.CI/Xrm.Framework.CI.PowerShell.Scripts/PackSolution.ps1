#
# PackSolution.ps1
#
param(
[string]$UnpackedFilesFolder,
[string]$MappingFile,
[string]$PackageType,
[bool]$UpdateVersion,
[string]$RequiredVersion,
[bool]$IncludeVersionInSolutionFile,
[bool]$IncrementReleaseVersion,
[string]$OutputPath,
[string]$sourceLoc,
[bool]$localize,
[bool]$TreatPackWarningsAsErrors,
[string]$CoreToolsPath,
[string]$LogsDirectory
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering PackSolution.ps1' -Verbose

#Parameters
Write-Verbose "UnpackedFilesFolder = $UnpackedFilesFolder"
Write-Verbose "MappingFile = $MappingFile"
Write-Verbose "PackageType = $PackageType"
Write-Verbose "UpdateVersion = $UpdateVersion"
Write-Verbose "RequiredVersion = $RequiredVersion"
Write-Verbose "IncludeVersionInSolutionFile = $IncludeVersionInSolutionFile"
Write-Verbose "IncrementReleaseVersion = $IncrementReleaseVersion"
Write-Verbose "OutputPath = $OutputPath"
Write-Verbose "SourceLoc = $sourceLoc"
Write-Verbose "Localize = $localize"
Write-Verbose "TreatPackWarningsAsErrors = $TreatPackWarningsAsErrors"
Write-Verbose "CoreToolsPath = $CoreToolsPath"
Write-Verbose "LogsDirectory = $LogsDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

$SolutionPackagerFile = $scriptPath + "\SolutionPackager.exe"
if ($CoreToolsPath)
{
	$SolutionPackagerFile = $CoreToolsPath + "\SolutionPackager.exe"
}

$PackParams = @{
	SolutionPackagerPath = $SolutionPackagerFile
	PackageType = $PackageType
	Folder = $UnpackedFilesFolder
	IncludeVersionInName = $IncludeVersionInSolutionFile
	IncrementReleaseVersion = $IncrementReleaseVersion
	TreatWarningsAsErrors = $TreatPackWarningsAsErrors
	OutputFolder = $OutputPath
}

if ($MappingFile)
{
	$PackParams.MappingFile = $MappingFile
}
if ($LogsDirectory)
{
	$PackParams.LogsDirectory = $LogsDirectory
}
if ($RequiredVersion)
{
	$PackParams.Version = $RequiredVersion
}
if ($sourceLoc)
{
	$PackParams.SourceLoc = $sourceLoc
}
if ($localize)
{
	$PackParams.Localize = $localize
}

Compress-XrmSolution @PackParams

Write-Verbose 'Leaving PackSolution.ps1'
