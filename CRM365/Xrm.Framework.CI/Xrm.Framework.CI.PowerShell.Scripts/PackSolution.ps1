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
[string]$OutputPath,
[bool]$TreatPackWarningsAsErrors
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering PackSolution.ps1' -Verbose

#Parameters
Write-Verbose "UnpackedFilesFolder = $UnpackedFilesFolder"
Write-Verbose "MappingFile = $MappingFile"
Write-Verbose "PackageType = $PackageType"
Write-Verbose "UpdateVersion = $UpdateVersion"
Write-Verbose "RequiredVersion = $RequiredVersion"
Write-Verbose "IncludeVersionInSolutionFile = $IncludeVersionInSolutionFile"
Write-Verbose "OutputPath = $OutputPath"
Write-Verbose "TreatPackWarningsAsErrors = $TreatPackWarningsAsErrors"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

if ($UpdateVersion)
{       
    Write-Verbose "Setting Solution Version in File to: $RequiredVersion"

	$SolutionXmlFile = "$UnpackedFilesFolder\Other\Solution.xml"

	Write-Verbose "Setting $SolutionXmlFile to IsReadyOnly = false"

	Set-ItemProperty $SolutionXmlFile -name IsReadOnly -value $false

	Write-Verbose "Setting Solution Version in File to: $RequiredVersion"

    Set-XrmSolutionVersionInFolder -SolutionFilesFolderPath $UnpackedFilesFolder -Version $RequiredVersion

    Write-Host "$SolutionXmlFile updated with $RequiredVersion"
}

$solutionInfo = Get-XrmSolutionInfoFromFolder -SolutionFilesFolderPath $UnpackedFilesFolder
$packSolutionName = $solutionInfo.UniqueName
$packSolutionVersion = $solutionInfo.Version
    
Write-Host "Packing Solution = " $packSolutionName ", Version = " $packSolutionVersion

$packStringBuilder = $packSolutionName
if ($IncludeVersionInSolutionFile)
{
    $packStringBuilder = $packStringBuilder + "_" + $packSolutionVersion.replace(".", "_")
}
$packManagedFile = $packStringBuilder + "_managed.zip"
$packUnmanagedFile = $packStringBuilder + ".zip"

$targetFile = $OutputPath + "\" + $packUnmanagedFile

$SolutionPackagerFile = $scriptPath + "\SolutionPackager.exe"

if ($MappingFile)
{
    $packOutput = & "$SolutionPackagerFile" /action:Pack /zipfile:"$targetFile" /folder:"$UnpackedFilesFolder" /packagetype:$PackageType /map:"$MappingFile"
}
else
{
    $packOutput = & "$SolutionPackagerFile" /action:Pack /zipfile:"$targetFile" /folder:"$UnpackedFilesFolder" /packagetype:$PackageType
}

Write-Output $packOutput

if ($lastexitcode -ne 0)
{
	throw "Solution Pack operation failed with exit code: $lastexitcode"
}
else
{
	if (($packOutput -ne $null) -and ($packOutput -like "*warnings encountered*"))
	{
		if ($TreatPackWarningsAsErrors)
		{
			throw "Solution Packager encountered warnings. Check the output."
		}
		else
		{
			Write-Warning "Solution Packager encountered warnings. Check the output."
		}
	}
	else
	{
		Write-Host "Solution Pack Completed Successfully"
	}
}

Write-Verbose 'Leaving PackSolution.ps1'
