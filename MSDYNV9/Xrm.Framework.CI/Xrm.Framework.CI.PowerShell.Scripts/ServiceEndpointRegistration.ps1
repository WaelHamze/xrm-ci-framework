#
# ServiceEndpointRegistration.ps1
#

param(
	[string]$CrmConnectionString,
	[string]$RegistrationType,
	[string]$MappingFile,
	[string]$SolutionName,
	[int]$Timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ServiceEndpointRegistration.ps1' -Verbose

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "RegistrationType = $RegistrationType"
Write-Verbose "MappingFile = $MappingFile"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Set-XrmServiceEndpointRegistration -RegistrationType $RegistrationType -MappingFile $MappingFile -SolutionName $SolutionName -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Host "Updated Service Endpoints and Steps"

Write-Verbose 'Leaving ServiceEndpointRegistration.ps1' -Verbose