#
# ApplySolution.ps1
#

param(
[string]$CrmConnectionString,
[string]$SolutionName,
[bool]$ImportAsync = $false,
[int]$AsyncWaitTimeout, #Optional - Async wait timeout in seconds
[int]$Timeout #Optional - CRM connection timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ApplySolution.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "ImportAsync = $ImportAsync"
Write-Verbose "AsyncWaitTimeout = $AsyncWaitTimeout"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Verbose "Upgrading Solution: $SolutionName"

Write-Host "Solution Upgrade Starting."

try
{
	$asyncOperationId = Merge-XrmSolution -ConnectionString "$CrmConnectionString" -UniqueSolutionName $SolutionName -Timeout $Timeout -ImportAsync $ImportAsync -AsyncWaitTimeout $AsyncWaitTimeout -WaitForCompletion $true -Verbose
}
finally
{
	Write-Host ("Async Job Id: {0}" –f $asyncOperationId)
}

 
Write-Host "Solution Upgrade Completed."

Write-Verbose 'Leaving ApplySolution.ps1'