#
# PackageFramework.ps1
#

$version = "8.0.19"

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
Copy-Item ($scriptPath + "\packages\Microsoft.CrmSdk.CoreTools.8.2.0.5\content\bin\coretools\SolutionPackager.exe") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Scripts\*.ps1") ($CIFrameworkRootDir) -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMPing") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkPackagesDir + "\MSCRMPing") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkPackagesDir + "\MSCRMPing") -Force -Recurse
Copy-Item ($CIFrameworkRootDir) ($CIFrameworkPackagesDir + "\MSCRMPing\ps_modules") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMPublishCustomizations") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkPackagesDir + "\MSCRMPublishCustomizations") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkPackagesDir + "\MSCRMPublishCustomizations") -Force -Recurse
Copy-Item ($CIFrameworkRootDir) ($CIFrameworkPackagesDir + "\MSCRMPublishCustomizations\ps_modules") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMPackSolution") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkPackagesDir + "\MSCRMPackSolution") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkPackagesDir + "\MSCRMPackSolution") -Force -Recurse
Copy-Item ($CIFrameworkRootDir) ($CIFrameworkPackagesDir + "\MSCRMPackSolution\ps_modules") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMExportSolution") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkPackagesDir + "\MSCRMExportSolution") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkPackagesDir + "\MSCRMExportSolution") -Force -Recurse
Copy-Item ($CIFrameworkRootDir) ($CIFrameworkPackagesDir + "\MSCRMExportSolution\ps_modules") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMImportSolution") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkPackagesDir + "\MSCRMImportSolution") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkPackagesDir + "\MSCRMImportSolution") -Force -Recurse
Copy-Item ($CIFrameworkRootDir) ($CIFrameworkPackagesDir + "\MSCRMImportSolution\ps_modules") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMPackageDeployer") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkPackagesDir + "\MSCRMPackageDeployer") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkPackagesDir + "\MSCRMPackageDeployer") -Force -Recurse
Copy-Item ($CIFrameworkRootDir) ($CIFrameworkPackagesDir + "\MSCRMPackageDeployer\ps_modules") -Force -Recurse
Copy-Item ($scriptPath + "\Lib\CrmSDKPowerShell\*.*") ($CIFrameworkPackagesDir + "\MSCRMPackageDeployer\ps_modules\xRMCIFramework") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMSetVersion") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkPackagesDir + "\MSCRMSetVersion") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkPackagesDir + "\MSCRMSetVersion") -Force -Recurse
Copy-Item ($CIFrameworkRootDir) ($CIFrameworkPackagesDir + "\MSCRMSetVersion\ps_modules") -Force -Recurse

Copy-Item ($scriptPath + "\Lib\CrmSDKPowerShell\*.*") ($CIFrameworkRootDir) -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Extension\*.*") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Extension\Screenshots") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Extension\Images") $CIFrameworkPackagesDir -Force -Recurse

tfx extension create --manifest-globs $xRMCIFrameworkPackageName --output-path $CIFrameworkPackagesDir --root $CIFrameworkPackagesDir

[void][Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" )
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory( $CIFrameworkRootDir, $xRMCIFrameworkPackagePath, $compressionLevel, $false )

Remove-Item $CIFrameworkTempDir -Force -Recurse
