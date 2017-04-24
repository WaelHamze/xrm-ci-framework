$version = "6.1.0.0"

Write-Host "Packaging xRM CI Framework $version"

$ErrorActionPreference = "Stop"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

$CIFrameworkPackagesDir = $scriptPath + "\Packages"
$CIFrameworkTempDir = $CIFrameworkPackagesDir + "\Temp"
$CIFrameworkRootDir = $CIFrameworkTempDir + "\xRMCIFramework"
$CIPowerShellDir = $CIFrameworkRootDir + "\PowerShell"
$CIBuildTemplatesDir = $CIFrameworkRootDir + "\BuildTemplates"
$CIBuildCustomAssembliesDir = $CIBuildTemplatesDir + "\CustomAssemblies"
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
New-Item $CIBuildTemplatesDir -ItemType directory
New-Item $CIBuildCustomAssembliesDir -ItemType directory

Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\microsoft.xrm.client.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\microsoft.xrm.sdk.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\microsoft.crm.sdk.proxy.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\Xrm.Framework.CI.PowerShell.dll") $CIPowerShellDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell\bin\Release\*.ps1") $CIPowerShellDir -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.TeamFoundation.Activities\bin\Release\microsoft.xrm.client.dll") $CIBuildCustomAssembliesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.TeamFoundation.Activities\bin\Release\microsoft.xrm.sdk.dll") $CIBuildCustomAssembliesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.TeamFoundation.Activities\bin\Release\microsoft.xrm.sdk.deployment.dll") $CIBuildCustomAssembliesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.TeamFoundation.Activities\bin\Release\microsoft.crm.sdk.proxy.dll") $CIBuildCustomAssembliesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.TeamFoundation.Activities\bin\Release\CustomActivitiesAndExtensions.xml") $CIBuildCustomAssembliesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.TeamFoundation.Activities\bin\Release\Xrm.Framework.CI.TeamFoundation.Activities.dll") $CIBuildCustomAssembliesDir -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.TeamFoundation.Templates\DynamicsCRM2013ReleaseTfvcTemplate.12.xaml") $CIBuildTemplatesDir -Force -Recurse

[Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" )
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory( $CIFrameworkRootDir, $xRMCIFrameworkPackagePath, $compressionLevel, $false )

Remove-Item $CIFrameworkTempDir -Force -Recurse