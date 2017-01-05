#
# BuildVSExtension.ps1
#

$version = "8.0.0"

Write-Host "Packaging xRM CI Framework VS Extension $version"

$ErrorActionPreference = "Stop"

#Script Location

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"

$CIFrameworkPackagesDir = $scriptPath + "\bin"

$CIFrameworkTempDir = $scriptPath + "\temp"

$CIFrameworkRootDir = $CIFrameworkTempDir + "\xRMCIFramework"

$CIFrameworkTasksDir = $CIFrameworkPackagesDir + "\tasks"

$xRMCIFrameworkPackageName = $CIFrameworkPackagesDir + "\vss-extension.json"

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

New-Item $CIFrameworkTasksDir -ItemType directory

Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\microsoft.xrm.sdk.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\microsoft.crm.sdk.proxy.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Xrm.Framework.CI.PowerShell.Cmdlets.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Xrm.Tooling.Connector.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.IdentityModel.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Management.Infrastructure.dll") $CIFrameworkRootDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets\bin\Release\Microsoft.Xrm.Sdk.Deployment.dll") $CIFrameworkRootDir -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMPing") $CIFrameworkTasksDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkTasksDir + "\MSCRMPing") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkTasksDir + "\MSCRMPing") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Scripts\Ping.ps1") ($CIFrameworkTasksDir + "\MSCRMPing") -Force -Recurse
Copy-Item ($CIFrameworkRootDir + "\*.*") ($CIFrameworkTasksDir + "\MSCRMPing") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Tasks\MSCRMPackSolution") $CIFrameworkTasksDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\icon.png") ($CIFrameworkTasksDir + "\MSCRMPackSolution") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Lib\ps_modules") ($CIFrameworkTasksDir + "\MSCRMPackSolution") -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.PowerShell.Scripts\PackSolution.ps1") ($CIFrameworkTasksDir + "\MSCRMPackSolution") -Force -Recurse
Copy-Item ($CIFrameworkRootDir + "\*.*") ($CIFrameworkTasksDir + "\MSCRMPackSolution") -Force -Recurse
Copy-Item ($scriptPath + "\packages\Microsoft.CrmSdk.CoreTools.8.2.0.2\content\bin\coretools\SolutionPackager.exe") ($CIFrameworkTasksDir + "\MSCRMPackSolution") -Force -Recurse

Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Extension\icon_128x128.png") $CIFrameworkPackagesDir -Force -Recurse
Copy-Item ($scriptPath + "\Xrm.Framework.CI.VSTS.BuildTasks\Extension\vss-extension.json") $CIFrameworkPackagesDir -Force -Recurse

tfx extension create --manifest-globs $xRMCIFrameworkPackageName --output-path $CIFrameworkPackagesDir

Remove-Item $CIFrameworkTempDir -Force -Recurse
