#
# DeployPackage.ps1
#

param(
[string]$CrmConnectionString,
[string]$PackageName,
[string]$PackageDirectory,
[string]$LogsDirectory = '',
[string]$PackageDeploymentPath,
[string]$Timeout = '00:30:00', #optional timeout for Import-CrmPackage, default to 1 hour and 20 min. See https://technet.microsoft.com/en-us/library/dn756301.aspx
[string]$RuntimePackageSettings,
[string]$UnpackFilesDirectory
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering DeployPackage.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"
Write-Verbose "PackageName = $PackageName"
Write-Verbose "PackageDirectory = $PackageDirectory"
Write-Verbose "LogsDirectory = $LogsDirectory"
Write-Verbose "PackageDeploymentPath = $PackageDeploymentPath"
Write-Verbose "Timeout = $Timeout"
Write-Verbose "RuntimePackageSettings = $RuntimePackageSettings"
Write-Verbose "UnpackFilesDirectory = $UnpackFilesDirectory"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Set Security Protocol
$currentProtocol = [System.Net.ServicePointManager]::SecurityProtocol
Write-Verbose "Current Security Protocol: $currentProtocol"
if (-not $currentProtocol.HasFlag([System.Net.SecurityProtocolType]::Tls11))
{
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol  -bor [System.Net.SecurityProtocolType]::Tls11;
}
if (-not $currentProtocol.HasFlag([System.Net.SecurityProtocolType]::Tls12))
{
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol  -bor [System.Net.SecurityProtocolType]::Tls12;
}
$currentProtocol = [System.Net.ServicePointManager]::SecurityProtocol
Write-Verbose "Modified Security Protocol: $currentProtocol"

#Load XRM Tooling

$crmToolingConnector = $scriptPath + "\Microsoft.Xrm.Tooling.CrmConnector.Powershell.dll"
$crmToolingDeployment = $scriptPath + "\Microsoft.Xrm.Tooling.PackageDeployment.Powershell.dll"

if ($PackageDeploymentPath)
{
	$crmToolingConnector = $PackageDeploymentPath + "\Microsoft.Xrm.Tooling.CrmConnector.Powershell.dll"
	$crmToolingDeployment = $PackageDeploymentPath + "\Microsoft.Xrm.Tooling.PackageDeployment.Powershell.dll"
}

Write-Verbose "Importing: $crmToolingConnector" 
Import-Module $crmToolingConnector
Write-Verbose "Imported: $crmToolingConnector"

Write-Verbose "Importing: $crmToolingDeployment" 
Import-Module $crmToolingDeployment
Write-Verbose "Imported: $crmToolingDeployment"

#Check Logs Directory
if ($LogsDirectory -eq '')
{
    $LogsDirectory = $PackageDirectory
}

#Create Connection

$CRMConn = Get-CrmConnection -ConnectionString $CrmConnectionString -Verbose

#Deploy Package

$PackageParams = @{
	CrmConnection = $CRMConn
	PackageDirectory = $PackageDirectory
	PackageName = $PackageName
	LogWriteDirectory = $LogsDirectory
	Timeout = $Timeout
}

if ($UnpackFilesDirectory)
{
	$PackageParams.UnpackFilesDirectory = $UnpackFilesDirectory
}

if ($RuntimePackageSettings)
{
	$PackageParams.RuntimePackageSettings = $RuntimePackageSettings
}

Import-CrmPackage -Verbose @PackageParams

Write-Verbose 'Leaving DeployPackage.ps1'
