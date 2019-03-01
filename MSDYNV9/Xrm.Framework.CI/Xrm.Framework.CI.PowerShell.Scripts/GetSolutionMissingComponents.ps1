#
# GetSolutionMissingComponents.ps1
#

[CmdletBinding()]

param(
[string]$solutionFile, #The absolute path to the solution file zip to be imported
[string]$crmConnectionString, #The target CRM organization connection string
[bool]$warnIfMissing, #Will generate a warning if missing components are found
[bool]$errorIfMissing,  #Will generate an error if missing components are found
[int]$Timeout #Optional - CRM connection timeout
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering GetSolutionMissingComponents.ps1'

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Verbose "solutionFile = $solutionFile"
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "warnIfMissing = $warnIfMissing"
Write-Verbose "errorIfMissing = $errorIfMissing"
Write-Verbose "Timeout = $Timeout"

$components = Get-XrmSolutionMissingComponents -ConnectionString "$CrmConnectionString" -SolutionFilePath "$solutionFile" -Timeout $Timeout

if ($components.Length -gt 0)
{
	$componentsJson = ConvertTo-Json -InputObject $components

	Write-Information -MessageData $componentsJson -Tags "XrmCIFramework"

	if ($warnIfMissing)
	{
		Write-Warning "$($components.Length) missing components found for solution"
	}
	if ($errorIfMissing)
	{
		throw "$($components.Length) missing components found for solution"
	}
}
 
Write-Verbose 'Leaving GetSolutionMissingComponents.ps1'
