#
# CreateSolution.ps1
#

param(
[string]$CrmConnectionString,
[string]$UniqueName,
[string]$DisplayName,
[string]$PublisherUniqueName,
[string]$VersionNumber = '1.0.0.0',
[string]$Description = '',
[int]$Timeout #Optional - CRM connection timeout
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering CreateSolution.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "UniqueName = $UniqueName"
Write-Verbose "DisplayName = $DisplayName"
Write-Verbose "PublisherUniqueName = $PublisherUniqueName"
Write-Verbose "VersionNumber = $VersionNumber"
Write-Verbose "Description = $Description"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Write-Verbose "Creating Solution: $DisplayName"

$SolutionParams = @{
	ConnectionString = $CrmConnectionString
	UniqueName = $UniqueName
	DisplayName = $DisplayName
	PublisherUniqueName = $PublisherUniqueName
	VersionNumber = $VersionNumber
	Description = $Description
	Timeout = $Timeout
}

$solutionId = Add-XrmSolution @SolutionParams

Write-Host ("Solution Created with Id: {0}" -f $solutionId)

Write-Verbose 'Leaving CreateSolution.ps1'