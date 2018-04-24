[CmdletBinding()]

param
(
    [string]$CrmConnectionString #The connection string as per CRM Sdk
)

$ErrorActionPreference = "Stop"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

Write-Verbose "ConnectionString = $connectionString"

if ($CrmConnectionString -eq '')
{
	$CrmConnectionString = $Env:CrmCon
}
$AssemblyName = 'Xrm.CI.Framework.Sample.Plugins'
$WebResourceFolderPath = "$scriptPath\WebResources"
$CommaSeparatedWebResourceExtensions = '*.html,*.js'
$RegExToMatchUniqueName = '$fileName'
$IncludeFileExtensionForUniqueName = $true
$Publish = $true
#$Timeout = 120

& "$scriptPath\..\packages\XrmCIFramework.9.0.0.17\tools\UpdateFoldersWebResources.ps1" -Verbose -CrmConnectionString "$CrmConnectionString" -WebResourceFolderPath $WebResourceFolderPath -CommaSeparatedWebResourceExtensions $CommaSeparatedWebResourceExtensions -RegExToMatchUniqueName $RegExToMatchUniqueName -IncludeFileExtensionForUniqueName $IncludeFileExtensionForUniqueName -Publish $true
