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

& "$scriptPath\..\..\Lib\xRMCIFramework\9.0.0\PackSolution.ps1" -Verbose -CoreToolsPath "$scriptPath\..\..\Lib\CoreTools\9.0.0\" -unpackedFilesFolder "$scriptPath\..\Customisations" -mappingFile "$scriptPath\mapping.xml" -PackageType Both -TreatPackWarningsAsErrors $true -UpdateVersion $false -OutputPath "C:\temp"
