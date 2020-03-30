#
# ExportCMData.ps1
#

[CmdletBinding()]

param(
[string]$crmConnectionString, #The target CRM organization connection string
[int]$crmConnectionTimeout, #CRM Connection Timeout in minutes
[string]$dataFile, #The absolute path of data.xml to create/update
[string]$schemaFile, #The absolute path to the schema file
[string]$logsDirectory, #Optional - will place the import log in here
[string]$configurationMigrationModulePath #The full path to the Configuration Migration PowerShell Module
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ExportCMData.ps1'

#Print Parameters

Write-Verbose "dataFile = $dataFile"
Write-Verbose "schemaFile = $schemaFile"
Write-Verbose "logsDirectory = $logsDirectory"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"
Write-Verbose "configurationMigrationModulePath = $configurationMigrationModulePath"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load PS Module
# Config Migration Module has its own connector references: Don't need to load XrmTooling Connector module

Write-Verbose "Importing Configuration Migration: $configurationMigrationModulePath"
Import-module "$configurationMigrationModulePath\Microsoft.Xrm.Tooling.ConfigurationMigration.psd1"

$connectParams = @{
	ConnectionString = "$crmConnectionString"
}
if ($crmConnectionTimeout -ne 0)
{
	$connectParams.MaxCrmConnectionTimeOutMinutes = $crmConnectionTimeout
}

if ($logsDirectory)
{
	$connectParams.LogWriteDirectory = $logsDirectory
}

#Obsolete -> CrmConnection can now take ConnStr direct
#$crmConn = Get-CrmConnection @connectParams -Verbose

$exportParams = @{
	CrmConnection = $crmConnectionString
	DataFile = "$dataFile"
	SchemaFile = "$schemaFile"
}
If ($logsDirectory)
{
	$exportParams.LogWriteDirectory = $logsDirectory
}

Export-CrmDataFile @exportParams -EmitLogToConsole -Verbose

Write-Verbose 'Leaving ExportCMData.ps1'