#
# Filename: RemoveSolution.ps1
#
param(
[string]$SolutionName, #The unique CRM solution name
[string]$CrmConnectionString, #The connection string as per CRM Sdk
[int]$Timeout=360
) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering RemoveSolution.ps1'

Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "ConnectionString = $CrmConnectionString"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

$solution = Get-XrmSolution -UniqueSolutionName $SolutionName -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

if ($solution -eq $null)
{
    Write-Warning "Solution is not currently installed."
}
else
{
	Write-Host "Removing Solution $SolutionName, Version: " + $solution.Version
	Remove-XrmRecord -EntityName "solution" -Id $solution.Id -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose
	Write-Host "Solution removed"
}

Write-Verbose 'Leaving RemoveSolution.ps1'