[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPackSolution.ps1'

#Get Parameters
$unpackedFilesFolder = Get-VstsInput -Name unpackedFilesFolder -Require
$mappingFile = Get-VstsInput -Name mappingFile
$packageType = Get-VstsInput -Name packageType -Require
$updateVersion = Get-VstsInput -Name updateVersion -AsBool
$includeVersionInSolutionFile = Get-VstsInput -Name includeVersionInSolutionFile -AsBool
$outputPath = Get-VstsInput -Name outputPath
$treatPackWarningsAsErrors = Get-VstsInput -Name treatPackWarningsAsErrors -AsBool
$crmSdkVersion = Get-VstsInput -Name crmSdkVersion -Require

#TFS Build Parameters
$buildNumber = $env:BUILD_BUILDNUMBER
$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY
$binariesDirectory = $env:BUILD_BINARIESDIRECTORY

#Print Verbose
Write-Verbose "unpackedFilesFolder = $unpackedFilesFolder"
Write-Verbose "mappingFile = $mappingFile"
Write-Verbose "packageType = $packageType"
Write-Verbose "updateVersion = $updateVersion"
Write-Verbose "includeVersionInSolutionFile = $includeVersionInSolutionFile"
Write-Verbose "treatPackWarningsAsErrors = $treatPackWarningsAsErrors"
Write-Verbose "buildNumber = $buildNumber"
Write-Verbose "sourcesDirectory = $sourcesDirectory"
Write-Verbose "binariesDirectory = $binariesDirectory"
Write-Verbose "crmSdkVersion = $crmSdkVersion"

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

$CoreToolsPath = "$mscrmToolsPath\CoreTools\$crmSdkVersion"

if ($mappingFile -eq $sourcesDirectory)
{
	$mappingFile = $null
}

if ($updateVersion)
{
	$versionNumber = $buildNumber.Substring($buildNumber.IndexOf("_") + 1)
}

& "$mscrmToolsPath\xRMCIFramework\$crmSdkVersion\PackSolution.ps1" -UnpackedFilesFolder $unpackedFilesFolder -MappingFile $mappingFile -PackageType $packageType -UpdateVersion $updateVersion -RequiredVersion $versionNumber -IncludeVersionInSolutionFile $includeVersionInSolutionFile -OutputPath $outputPath -TreatPackWarningsAsErrors $treatPackWarningsAsErrors -CoreToolsPath $CoreToolsPath

Write-Verbose 'Leaving MSCRMPackSolution.ps1'
