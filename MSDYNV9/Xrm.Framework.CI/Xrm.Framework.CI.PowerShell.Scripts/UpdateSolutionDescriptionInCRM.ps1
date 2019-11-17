#
# Filename: UpdateSolutionDescriptionInCRM.ps1
#
param(
[string]$SolutionName, #The unique CRM solution name
[string]$CrmConnectionString, #The connection string as per CRM Sdk
[int]$Timeout=360,
[string]$NewDescription, #The new description value to be applied to the solution
[string]$DescriptionUpdateMethod #The method to update the description
) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateSolutionDescriptionInCRM.ps1'

Write-Verbose "SolutionName = $SolutionName"
Write-Verbose "ConnectionString = $CrmConnectionString"
Write-Verbose "Timeout = $Timeout"
Write-Verbose "NewDescription = $NewDescription"
Write-Verbose "DescriptionUpdateMethod = $DescriptionUpdateMethod"

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
    Write-Error "Solution is not currently installed."
}

#Create the Description String
$updatedDescriptionValue = ""
switch ($DescriptionUpdateMethod.ToUpperInvariant()) {
    'REPLACE' {
		 $updatedDescriptionValue = $NewDescription 
	}
    'APPENDTOTOP' { 
		$updatedDescriptionValue = $NewDescription + $solution.Description
	}
    'APPENDTOBOTTOM' { 
		$updatedDescriptionValue = $solution.Description + $NewDescription
	}
    default {
		$updatedDescriptionValue = $solution.Description
        Write-Error "$DescriptionUpdateMethod is not a valid input"
    }
}

#Update the description
Write-Host "Updating solution description to:"
Write-Host "$updatedDescriptionValue"

$updateSolution = New-XrmEntity -EntityName "solution"
$updateSolution.Id = $solution.Id
$updateSolution["description"] = $updatedDescriptionValue
Set-XrmEntity -ConnectionString $CrmConnectionString -EntityObject $updateSolution
Write-Host "Solution description updated"

Write-Verbose 'Leaving UpdateSolutionDescriptionInCRM.ps1'
