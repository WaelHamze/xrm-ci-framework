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
[string]$configurationMigrationModulePath, #The full path to the Configuration Migration PowerShell Module
[string]$toolingConnectorModulePath #The full path to the Tooling Connector PowerShell Module
) 

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering ExporCMtData.ps1'

#Print Parameters

Write-Verbose "dataFile = $dataFile"
Write-Verbose "schemaFile = $schemaFile"
Write-Verbose "logsDirectory = $logsDirectory"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"
Write-Verbose "configurationMigrationModulePath = $configurationMigrationModulePath"
Write-Verbose "toolingConnectorModulePath = $toolingConnectorModulePath"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load PS Module
Write-Verbose "Importing Configuration Migration: $PowerAppsCheckerPath"
Import-module "$ConfigurationMigrationModulePath\Microsoft.Xrm.Tooling.ConfigurationMigration.psd1"

Write-Verbose "Import Tooling Connector: $ToolingConnectorModulePath"
Import-module "$ToolingConnectorModulePath\Microsoft.Xrm.Tooling.CrmConnector.PowerShell.dll"

$connectParams = @{
	ConnectionString = "$CrmConnectionString"
}
if ($crmConnectionTimeout -ne 0)
{
	$connectParams.MaxCrmConnectionTimeOutMinutes = $crmConnectionTimeout
}

if ($logsDirectory)
{
	$connectParams.LogWriteDirectory = $logsDirectory
}

$CRMConn = Get-CrmConnection @connectParams -Verbose

$exportParams = @{
	CrmConnection = $CRMConn
	DataFile = "$dataFile"
	SchemaFile = "$schemaFile"
}
If ($logsDirectory)
{
	$exportParams.LogWriteDirectory = $logsDirectory
}

Export-CrmDataFile @exportParams -EmitLogToConsole -Verbose

Write-Verbose 'Leaving ExporCMtData.ps1'