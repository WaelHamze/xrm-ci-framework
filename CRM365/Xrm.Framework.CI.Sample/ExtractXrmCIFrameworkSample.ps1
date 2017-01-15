[CmdletBinding()]

param
(
	[string]$connectionString #The connection string as per CRM Sdk
)

$ErrorActionPreference = "Stop"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

Write-Verbose "ConnectionString = $connectionString"

& "$scriptPath\Lib\xRMCIFramework\ExtractCustomizations.ps1" -Verbose -solutionPackager "$scriptPath\Lib\xRMCIFramework\SolutionPackager.exe" -solutionFilesFolder "$scriptPath\SolutionFiles" -mappingFile "$scriptPath\XrmCIFrameworkSampleMapping.xml" -solutionName "XrmCIFrameworkSample" -connectionString $connectionString -TreatPackWarningsAsErrors $true