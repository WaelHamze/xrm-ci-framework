#
# Make.ps1
# 
# Run this script to generate the tasks folder structure and extension
#

$ErrorActionPreference = "Stop"

Write-Host "Packaging Dynamics 365 Builds tools"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Host "Script Path: $scriptPath"


#Creating output directory
$OutputDir = $scriptPath + "\bin"

if (Test-Path $OutputDir)
{
	Remove-Item $OutputDir -Force -Recurse
}

New-Item $OutputDir -ItemType directory | Out-Null

#Creating temp directory
$TempDir = $scriptPath + "\temp"

if (Test-Path $TempDir)
{
	Remove-Item $TempDir -Force -Recurse
}

New-Item $TempDir -ItemType directory | Out-Null

#Copy Extension Files
Copy-Item .\icon_128x128.png $OutputDir -Force -Recurse
Copy-Item .\license.txt $OutputDir -Force -Recurse
Copy-Item .\overview.md $OutputDir -Force -Recurse
Copy-Item .\vss-extension.json $OutputDir -Force -Recurse
Copy-Item .\Images $OutputDir -Force -Recurse
Copy-Item .\Screenshots $OutputDir -Force -Recurse

#Copy Initial Tasks
Copy-Item -Path .\Tasks -Destination $OutputDir -Recurse

#MSCRMToolInstaller
$taskName = "MSCRMToolInstaller"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"
New-Item "$OutputDir\Tasks\$taskName\Lib\xRMCIFramework\8.2.0" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\xRMCIFramework\9.0.0\*.* -Destination "$OutputDir\Tasks\$taskName\Lib\xRMCIFramework\8.2.0"
New-Item "$OutputDir\Tasks\$taskName\Lib\xRMCIFramework\9.0.0" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\xRMCIFramework\9.0.0\*.* -Destination "$OutputDir\Tasks\$taskName\Lib\xRMCIFramework\9.0.0"
New-Item "$OutputDir\Tasks\$taskName\Lib\CoreTools\8.2.0" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\Microsoft.CrmSdk.CoreTools\8.2.0\SolutionPackager.exe -Destination "$OutputDir\Tasks\$taskName\Lib\CoreTools\8.2.0"
New-Item "$OutputDir\Tasks\$taskName\Lib\CoreTools\9.0.0" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\Microsoft.CrmSdk.CoreTools\9.0.0\SolutionPackager.exe -Destination "$OutputDir\Tasks\$taskName\Lib\CoreTools\9.0.0"
New-Item "$OutputDir\Tasks\$taskName\Lib\OnlineManagementAPI\1.0.0" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\Microsoft.Xrm.OnlineManagementAPI\1.0.0\*.* -Destination "$OutputDir\Tasks\$taskName\Lib\OnlineManagementAPI\1.0.0"
New-Item "$OutputDir\Tasks\$taskName\Lib\PackageDeployment\8.2.0" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\Microsoft.CrmSdk.XrmTooling.PackageDeployment.PowerShell\8.2.0\*.* -Destination "$OutputDir\Tasks\$taskName\Lib\PackageDeployment\8.2.0"
New-Item "$OutputDir\Tasks\$taskName\Lib\PackageDeployment\9.0.0" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\Microsoft.CrmSdk.XrmTooling.PackageDeployment.PowerShell\9.0.0\*.* -Destination "$OutputDir\Tasks\$taskName\Lib\PackageDeployment\9.0.0"

#MSCRMApplySolution
$taskName = "MSCRMApplySolution"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMCopySolutionComponents
$taskName = "MSCRMCopySolutionComponents"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMRemoveSolutionComponents
$taskName = "MSCRMRemoveSolutionComponents"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#Ping
$taskName = "MSCRMPing"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMPublishCustomizations
$taskName = "MSCRMPublishCustomizations"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMPackSolution
$taskName = "MSCRMPackSolution"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMExportSolution
$taskName = "MSCRMExportSolution"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMExtractSolution
$taskName = "MSCRMExtractSolution"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMImportSolution
$taskName = "MSCRMImportSolution"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMPackageDeployer
$taskName = "MSCRMPackageDeployer"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMSetVersion
$taskName = "MSCRMSetVersion"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMUpdateSecureConfiguration
$taskName = "MSCRMUpdateSecureConfiguration"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMBackupOnlineInstance
$taskName = "MSCRMBackupOnlineInstance"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMProvisionOnlineInstance
$taskName = "MSCRMProvisionOnlineInstance"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMGetOnlineInstanceByName
$taskName = "MSCRMGetOnlineInstanceByName"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMDeleteOnlineInstance
$taskName = "MSCRMDeleteOnlineInstance"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMRestoreOnlineInstance
$taskName = "MSCRMRestoreOnlineInstance"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMSetOnlineInstanceAdminMode
$taskName = "MSCRMSetOnlineInstanceAdminMode"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMUpdatePluginAssembly
$taskName = "MSCRMUpdatePluginAssembly"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMUpdateWebResource
$taskName = "MSCRMUpdateWebResources"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMPluginRegistration
$taskName = "MSCRMPluginRegistration"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#MSCRMSplitPluginAssembly
$taskName = "MSCRMSplitPluginAssembly"
Copy-Item -Path .\icon.png -Destination "$OutputDir\Tasks\$taskName"
New-Item "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk" -ItemType directory | Out-Null
Copy-Item -Path .\Lib\VstsTaskSdk\0.10.0\*.* -Destination "$OutputDir\Tasks\$taskName\ps_modules\VstsTaskSdk"

#Clean Up
Remove-Item $TempDir -Force -Recurse
