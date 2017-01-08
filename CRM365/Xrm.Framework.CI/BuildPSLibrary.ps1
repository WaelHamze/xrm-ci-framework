#
# BuildPSLibrary.ps1
#

$version = "8.0.0.0"

Write-Host "Packaging xRM CI Framework PowerShell $version"

$ErrorActionPreference = "Stop"

#Script Location

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

$CIFrameworkPackagesDir = $scriptPath + "\bin"

$CIFrameworkTempDir = $scriptPath + "\temp"

$CIFrameworkRootDir = $CIFrameworkTempDir + "\xRMCIFramework"

$xRMCIFrameworkPackageName = "xRMCIFramework_" + $version + ".zip"

$xRMCIFrameworkPackagePath = $CIFrameworkPackagesDir + "\" + $xRMCIFrameworkPackageName

if (Test-Path $CIFrameworkTempDir)
{
	Remove-Item $CIFrameworkTempDir -Force -Recurse
}

if (Test-Path $CIFrameworkPackagesDir)
{
	Remove-Item $CIFrameworkPackagesDir -Force -Recurse
}

New-Item $CIFrameworkTempDir -ItemType directory

New-Item $CIFrameworkRootDir -ItemType directory

New-Item $CIFrameworkPackagesDir -ItemType directory

Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\microsoft.xrm.sdk.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\microsoft.crm.sdk.proxy.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Xrm.Framework.CI.PowerShell.Cmdlets.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Xrm.Tooling.Connector.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.IdentityModel.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Management.Infrastructure.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Xrm.Sdk.Deployment.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.IdentityModel.Clients.ActiveDirectory.dll") $CIFrameworkRootDir -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Scripts\*.ps1") $CIFrameworkRootDir -Force -Recurse



[Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" )

$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal

[System.IO.Compression.ZipFile]::CreateFromDirectory( $CIFrameworkRootDir, $xRMCIFrameworkPackagePath, $compressionLevel, $false )

Remove-Item $CIFrameworkTempDir -Force -Recurse
