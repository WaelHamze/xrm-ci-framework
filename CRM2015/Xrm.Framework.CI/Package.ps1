$version = "7.1.0.0"

Write-Host "Packaging xRM CI Framework $version"

$ErrorActionPreference = "Stop"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

$CIFrameworkPackagesDir = $scriptPath + "\Releases"
$CIFrameworkTempDir = $CIFrameworkPackagesDir + "\Temp"
$CIFrameworkRootDir = $CIFrameworkTempDir + "\xRMCIFramework"
$CIPowerShellDir = $CIFrameworkRootDir + "\PowerShell"
$xRMCIFrameworkPackageName = "xRMCIFramework_" + $version + ".zip"
$xRMCIFrameworkPackagePath = $CIFrameworkPackagesDir + "\" + $xRMCIFrameworkPackageName

if (Test-Path $CIFrameworkTempDir)
{
	Remove-Item $CIFrameworkTempDir -Force -Recurse
}

if (Test-Path $xRMCIFrameworkPackagePath)
{
	Remove-Item $xRMCIFrameworkPackagePath -Force -Recurse
}

New-Item $CIFrameworkTempDir -ItemType directory
New-Item $CIFrameworkRootDir -ItemType directory
New-Item $CIPowerShellDir -ItemType directory

Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\microsoft.xrm.client.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\microsoft.xrm.sdk.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\microsoft.crm.sdk.proxy.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\Xrm.Framework.CI.PowerShell.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\Scripts\*.ps1") $CIPowerShellDir -Force -Recurse

[Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" )
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory( $CIFrameworkRootDir, $xRMCIFrameworkPackagePath, $compressionLevel, $false )

Remove-Item $CIFrameworkTempDir -Force -Recurse