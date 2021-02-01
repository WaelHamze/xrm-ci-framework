#
# UpdateConnectionReferences.ps1
#
param(
[string]$CrmConnectionString,
[string]$ConnectionReferencesJson
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateConnectionReferences.ps1'

#Parameters
Write-Verbose "$ConnectionReferencesJson = $ConnectionReferencesJson"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit"
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

#load json string into array
$array = ConvertFrom-Json $ConnectionReferencesJson

Write-Host ("Processing ({0}) records" -f $array.Count)

#iterate through the environment variables
For ($i=0; $i -lt $array.Count; $i++)
{
	$name = $array[$i][0]
	$connectionId = $array[$i][1]

	Write-Host ("Updating connection reference with name: {0}" -f $name)

	Set-XrmConnectionReference -ConnectionString $CrmConnectionString -Name $name -ConnectionId "$connectionId"

	Write-Host ("Updated connection reference to connection: {0}" -f $connectionId)
}

Write-Host "UpdateConnectionReferences Update Succeeded"

Write-Verbose 'Leaving UpdateConnectionReferences.ps1'