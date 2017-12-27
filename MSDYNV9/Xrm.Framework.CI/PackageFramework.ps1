#
# PackageFramework.ps1
#

$version = "9.0.0"

Write-Host "Packaging xRM CI Framework $version"

$ErrorActionPreference = "Stop"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

$CIFrameworkPackagesDir = $scriptPath + "\bin"
$CIFrameworkTempDir = $scriptPath + "\temp"
$CIFrameworkRootDir = $CIFrameworkTempDir + "\xRMCIFramework"
$xRMCIFrameworkPackageName = $CIFrameworkPackagesDir + "\vss-extension.json"
$xRMCIFrameworkPackageFile = "xRMCIFramework_" + $version + ".zip"
$xRMCIFrameworkPackagePath = $CIFrameworkPackagesDir + "\" + $xRMCIFrameworkPackageFile

if (Test-Path $CIFrameworkTempDir)
{
	Remove-Item $CIFrameworkTempDir -Force -Recurse
}

if (Test-Path $CIFrameworkPackagesDir)
{
	Remove-Item $CIFrameworkPackagesDir -Force -Recurse
}

New-Item $CIFrameworkTempDir -ItemType directory | Out-Null

New-Item $CIFrameworkRootDir -ItemType directory | Out-Null

New-Item $CIFrameworkPackagesDir -ItemType directory | Out-Null

Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\microsoft.xrm.sdk.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\microsoft.crm.sdk.proxy.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Xrm.Framework.CI.PowerShell.Cmdlets.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Xrm.Tooling.Connector.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.IdentityModel.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Management.Infrastructure.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Xrm.Sdk.Deployment.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.IdentityModel.Clients.ActiveDirectory.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\packages\Microsoft.CrmSdk.CoreTools.9.0.0.7\content\bin\coretools\SolutionPackager.exe") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Scripts\*.ps1") ($CIFrameworkRootDir) -Force -Recurse

[void][Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" )
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory( $CIFrameworkRootDir, $xRMCIFrameworkPackagePath, $compressionLevel, $false )

Remove-Item $CIFrameworkTempDir -Force -Recurse
