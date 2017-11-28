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

& "$scriptPath\..\..\Lib\xRMCIFramework\9.0.0\ExtractCustomizations.ps1" -Verbose -solutionPackager "$scriptPath\..\..\Lib\CoreTools\9.0.0\SolutionPackager.exe" -solutionFilesFolder "$scriptPath\..\Customisations" -mappingFile "$scriptPath\mapping.xml" -solutionName "xRMCISample" -connectionString $connectionString -TreatPackWarningsAsErrors $true
