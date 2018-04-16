#
# Filename: ExtractSolution.ps1
#
param(
[string]$FromSolutionName="TestPluginRegistration", #The unique CRM solution name
[string]$ToSolutionName="Test1", #The unique CRM solution name
[string]$CrmConnectionString="AuthType=Office365;Username=sagarh@dracola.onmicrosoft.com; Password=Sagar@123;Url=https://draco2.crm8.dynamics.com", #The connection string as per CRM Sdk
[int]$Timeout=360
) 

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MoveSolutionComponent.ps1'

Write-Verbose "FromSolutionName = $FromSolutionName"
Write-Verbose "ToSolutionName = $ToSolutionName"
Write-Verbose "ConnectionString = $CrmConnectionString"
Write-Verbose "Timeout = $Timeout"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
$xrmCIToolkit ="C:\Users\Sagar Gaikwad\source\repos\xrm-ci-framework\MSDYNV9\Xrm.Framework.CI\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Debug\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

Move-XrmSolutionComponents -FromSolutionName $FromSolutionName -ToSolutionName $ToSolutionName -ConnectionString $CrmConnectionString -Timeout $Timeout -Verbose

Write-Verbose 'Leaving MoveSolutionComponent.ps1'