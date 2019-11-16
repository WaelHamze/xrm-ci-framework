#
# UpdateEnvironmentVariables.ps1
#
param(
[string]$CrmConnectionString,
[string]$EnvironmentVariablesJson
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateEnvironmentVariables.ps1'

#Parameters
Write-Verbose "EnvironmentVariablesJson = $EnvironmentVariablesJson"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit"
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

#load json string into array
$array = ConvertFrom-Json $EnvironmentVariablesJson

Write-Host ("Processing ({0}) records" -f $array.Count)

#iterate through the environment variables
For ($i=0; $i -lt $array.Count; $i++)
{
	$name = $array[$i][0]
	$value = $array[$i][1]

	Write-Host ("Updating environment variable with name: {0}" -f $name)

	Set-XrmEnvironmentVariableValue -ConnectionString $CrmConnectionString -Name $name -Value "$value"

	Write-Host ("Updated environment variable with name: {0}" -f $value)
}

Write-Host "UpdateEnvironmentVariables Update Succeeded"

Write-Verbose 'Leaving UpdateEnvironmentVariables.ps1'