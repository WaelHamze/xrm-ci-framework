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
[bool]$TreatUnpackWarningsAsErrors
) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ExtractSolution.ps1'

Write-Verbose "UnpackedFilesFolder = $UnpackedFilesFolder"
Write-Verbose "MappingFile = $mappingFile"
Write-Verbose "PackageType = $PackageType"
Write-Verbose "SolutionName = $solutionName"
Write-Verbose "ConnectionString = $connectionString"
Write-Verbose "SolutionFile = $solutionFile"
Write-Verbose "CoreToolsPath = $CoreToolsPath"
Write-Verbose "TreatUnpackWarningsAsErrors = $TreatUnpackWarningsAsErrors"

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
	# CI Toolkit
	$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
	$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
	Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
	Import-Module $xrmCIToolkit
	
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

#Solution Packager
if ($MappingFile)
{
	$extractOuput = & "$SolutionPackagerFile" /action:Extract /zipfile:"$solutionFile" /folder:"$UnpackedFilesFolder" /packagetype:$PackageType /errorlevel:Info /allowWrite:Yes /allowDelete:Yes /map:$mappingFile
}
else
{
	$extractOuput = & "$SolutionPackagerFile" /action:Extract /zipfile:"$solutionFile" /folder:"$UnpackedFilesFolder" /packagetype:$PackageType /errorlevel:Info /allowWrite:Yes /allowDelete:Yes
}
Write-Output $extractOuput

if ($lastexitcode -ne 0)
{
    throw "Solution Extract operation failed with exit code: $lastexitcode"
}
else
{
    if (($extractOuput -ne $null) -and ($extractOuput -like "*warnings encountered*"))
    {
        if ($TreatUnpackWarningsAsErrors)
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
		Write-Host "Solution Extract Completed Successfully"
	}
}

# End of script

Write-Verbose 'Leaving ExtractSolution.ps1'