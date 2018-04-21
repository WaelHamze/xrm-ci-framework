#
# ApplySolution.ps1
#

param(
[string]$CrmConnectionString,
[string]$SolutionName,
[int]$AsyncWaitTimeout, #Optional - Async wait timeout in seconds
[int]$Timeout #Optional - CRM connection timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ApplySolution.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "SolutionName = $SolutionName"
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

$importJobId = [guid]::NewGuid()

Write-Host "Solution Upgrade Starting. Import Job Id: $importJobId"
  
$asyncOperationId = Merge-XrmSolution -ConnectionString "$CrmConnectionString" -UniqueSolutionName $SolutionName -ImportJobId $importJobId -ImportAsync $true -AsyncWaitTimeout $AsyncWaitTimeout  -Timeout $Timeout -Verbose
 
Write-Host "Solution Upgrade Completed. Import Job Id: $importJobId"

Write-Verbose 'Leaving ApplySolution.ps1'