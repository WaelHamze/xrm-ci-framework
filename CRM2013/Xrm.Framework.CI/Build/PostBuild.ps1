
$CIFrameworkDir = $env:TF_BUILD_BINARIESDIRECTORY + "\" + $env:TF_BUILD_BUILDNUMBER
$CIPowerShellDir = $CIFrameworkDir + "\PowerShell"
$CIBuildTemplatesDir = $CIFrameworkDir + "\BuildTemplates"
$CIBuildCustomAssembliesDir = $CIBuildTemplatesDir + "\CustomAssemblies"

New-Item $CIPowerShellDir -ItemType directory
New-Item $CIBuildTemplatesDir -ItemType directory
New-Item $CIBuildCustomAssembliesDir -ItemType directory

Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\microsoft.xrm.client.dll") $CIPowerShellDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\microsoft.xrm.sdk.dll") $CIPowerShellDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\microsoft.crm.sdk.proxy.dll") $CIPowerShellDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\Xrm.Framework.CI.PowerShell.dll") $CIPowerShellDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\*.ps1") $CIPowerShellDir -Force -Recurse -Verbose

Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\microsoft.xrm.client.dll") $CIBuildCustomAssembliesDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\microsoft.xrm.sdk.dll") $CIBuildCustomAssembliesDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\microsoft.crm.sdk.proxy.dll") $CIBuildCustomAssembliesDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\microsoft.xrm.sdk.deployment.dll") $CIBuildCustomAssembliesDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\CustomActivitiesAndExtensions.xml") $CIBuildCustomAssembliesDir -Force -Recurse -Verbose
Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\Xrm.Framework.CI.TeamFoundation.Activities.dll") $CIBuildCustomAssembliesDir -Force -Recurse -Verbose

Copy-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\DynamicsCRM2013ReleaseTfvcTemplate.12.xaml") $CIBuildTemplatesDir -Force -Recurse -Verbose

[Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" )
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
[System.IO.Compression.ZipFile]::CreateFromDirectory( $CIFrameworkDir, $CIFrameworkDir + ".zip", $compressionLevel, $false )

Remove-Item ($env:TF_BUILD_BINARIESDIRECTORY + "\*.*") -exclude "*.zip" -Force -Recurse -Verbose