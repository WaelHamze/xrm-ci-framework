#
# GetSolutionMissingDependencies.ps1
#

[CmdletBinding()]

param(
[string]$solutionName, #The name of the solution to be checked
[string]$crmConnectionString, #The target CRM organization connection string
[bool]$warnIfMissing, #Will generate a warning if missing components are found
[bool]$errorIfMissing,  #Will generate an error if missing components are found
[int]$Timeout #Optional - CRM connection timeout
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

Write-Verbose 'Entering GetSolutionMissingDependencies.ps1'

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Verbose "solutionName = $SolutionName"
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "warnIfMissing = $warnIfMissing"
Write-Verbose "errorIfMissing = $errorIfMissing"
Write-Verbose "Timeout = $Timeout"

$dependencies = Get-XrmSolutionMissingDependencies -ConnectionString "$CrmConnectionString" -UniqueSolutionName "$solutionName" -Timeout $Timeout

if ($dependencies.Entities.Count -gt 0)
{
	$dependenciesJson = ConvertTo-Json -InputObject $dependencies

	Write-Information -MessageData $dependenciesJson -Tags "XrmCIFramework"

	if ($warnIfMissing)
	{
		Write-Warning "$($dependencies.Entities.Count) missing dependencies found for solution"
	}
	if ($errorIfMissing)
	{
		throw "$($dependencies.Entities.Count) missing dependencies found for solution"
	}
}
 
Write-Verbose 'Leaving GetSolutionMissingDependencies.ps1'
