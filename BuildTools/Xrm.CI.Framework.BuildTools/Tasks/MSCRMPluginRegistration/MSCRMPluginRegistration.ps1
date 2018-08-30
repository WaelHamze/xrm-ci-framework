[CmdletBinding()]

param()

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering MSCRMPluginRegistration.ps1'

#Get Parameters
$crmConnectionString = Get-VstsInput -Name crmConnectionString -Require
$registrationType = Get-VstsInput -Name registrationType -Require
$assemblyPath = Get-VstsInput -Name assemblyPath -Require
$useSplitAssembly = Get-VstsInput -Name useSplitAssembly -AsBool
$projectFilePath = Get-VstsInput -Name projectFilePath 
$MappingFile = Get-VstsInput -Name mappingFile
$solutionName = Get-VstsInput -Name solutionName
$crmConnectionTimeout = Get-VstsInput -Name crmConnectionTimeout -Require -AsInt

#Print Verbose
Write-Verbose "crmConnectionString = $crmConnectionString"
Write-Verbose "registrationType = $registrationType"
Write-Verbose "assemblyPath = $assemblyPath"
Write-Verbose "projectFilePath = $projectFilePath"
Write-Verbose "useSplitAssembly = $useSplitAssembly"
Write-Verbose "MappingFile = $MappingFile"
Write-Verbose "solutionName = $solutionName"
Write-Verbose "crmConnectionTimeout = $crmConnectionTimeout"

#MSCRM Tools
$mscrmToolsPath = $env:MSCRM_Tools_Path
Write-Verbose "MSCRM Tools Path: $mscrmToolsPath"

if (-not $mscrmToolsPath)
{
	Write-Error "MSCRM_Tools_Path not found. Add 'MSCRM Tool Installer' before this task."
}

& "$mscrmToolsPath\xRMCIFramework\9.0.0\PluginRegistration.ps1" -CrmConnectionString $crmConnectionString -RegistrationType $registrationType -AssemblyPath $assemblyPath -MappingFile $MappingFile -SolutionName $solutionName -useSplitAssembly $useSplitAssembly -projectFilePath $projectFilePath -Timeout $crmConnectionTimeout

Write-Verbose 'Leaving MSCRMPluginRegistration.ps1'