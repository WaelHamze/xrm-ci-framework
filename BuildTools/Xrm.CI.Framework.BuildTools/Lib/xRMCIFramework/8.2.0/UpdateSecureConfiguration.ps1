#
# UpdateSecureConfiguration.ps1
#
param(
[string]$CrmConnectionString,
[string]$SecureConfiguration
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateSecureConfiguration.ps1'

#Parameters
Write-Verbose "CrmConnectionString = $CrmConnectionString"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit"
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

#load json string into array
$array = ConvertFrom-Json $SecureConfiguration

#iterate through the configuration items and set secure configuration
For ($i=0; $i -lt $array.Count; $i++)
{
	$secureConfigGuid = $array[$i][0]
	$secureConfig =  $array[$i][1]

    $step = Get-XrmEntity -ConnectionString $CrmConnectionString -Id $secureConfigGuid -EntityName "sdkmessageprocessingstep"

    if(!$step.Attributes.Contains("sdkmessageprocessingstepsecureconfigid"))
    {
        $processingStepSecureConfiguration = New-XrmEntity -EntityName "sdkmessageprocessingstepsecureconfig"
	    $processingStepSecureConfiguration.Attributes.Add("secureconfig", $secureConfig)
        $result = Add-XrmEntity -ConnectionString $CrmConnectionString -EntityObject $processingStepSecureConfiguration
        $processingStepSecureConfiguration.Id = $result.Guid
        $step.Attributes["sdkmessageprocessingstepsecureconfigid"] = $processingStepSecureConfiguration.ToEntityReference()
        Set-XrmEntity -ConnectionString $CrmConnectionString -EntityObject $step
	}
	else
    {
		$secureConfigurationReference = $step.Attributes["sdkmessageprocessingstepsecureconfigid"]
        $processingStepSecureConfiguration = Get-XrmEntity -ConnectionString $CrmConnectionString -EntityName $secureConfigurationReference.LogicalName -Id $secureConfigurationReference.Id
        $processingStepSecureConfiguration.Attributes["secureconfig"] = $secureConfig
        Set-XrmEntity -ConnectionString $CrmConnectionString -EntityObject $processingStepSecureConfiguration
    }
}

Write-Host "Secure Configuration Update Succeeded"

Write-Verbose 'Leaving UpdateSecureConfiguration.ps1'