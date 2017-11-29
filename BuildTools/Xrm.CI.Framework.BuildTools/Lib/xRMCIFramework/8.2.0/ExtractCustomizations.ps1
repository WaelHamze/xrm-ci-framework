#
# Filename: ExtractCustomizations.ps1
#
param([string]$solutionPackager, #The full path to the solutionpackager.exe
[string]$solutionFilesFolder, #The folder to extract the CRM solution
[string]$mappingFile, #The full path to the mapping file
[string]$solutionName, #The unique CRM solution name
[string]$connectionString, #The connection string as per CRM Sdk
[bool]$TreatPackWarningsAsErrors) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ExtractCustomizations.ps1'

Write-Verbose "Solution Packager = $solutionPackager"
Write-Verbose "Solution Files Folder = $solutionFilesFolder"
Write-Verbose "Mapping File = $mappingFile"
Write-Verbose "ConnectionString = $connectionString"
Write-Verbose "TreatPackWarningsAsErrors = $TreatPackWarningsAsErrors"

# CI Toolkit
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit

#Export Solutions
Write-Output "Exporting Solutions to: " $env:TEMP
$unmanagedSolution = Export-XrmSolution -ConnectionString $connectionString -Managed $False -OutputFolder $env:TEMP -UniqueSolutionName $solutionName
Write-Output "Exported Solution: $unmanagedSolution"
$managedSolution = Export-XrmSolution -ConnectionString $connectionString -Managed $True -OutputFolder $env:TEMP -UniqueSolutionName $solutionName
Write-Output "Exported Solution: $managedSolution"

#Solution Packager
$extractOuput = & "$solutionPackager" /action:Extract /zipfile:"$env:TEMP\$unmanagedSolution" /folder:"$solutionFilesFolder" /packagetype:Both /errorlevel:Info /allowWrite:Yes /allowDelete:Yes /map:$mappingFile
Write-Output $extractOuput
if ($lastexitcode -ne 0)
{
    throw "Solution Extract operation failed with exit code: $lastexitcode"
}
else
{
    if (($extractOuput -ne $null) -and ($extractOuput -like "*warnings encountered*"))
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

# End of script

Write-Verbose 'Leaving ExtractCustomizations.ps1'