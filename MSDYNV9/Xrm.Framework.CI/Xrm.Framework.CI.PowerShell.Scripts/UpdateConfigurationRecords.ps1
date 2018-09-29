#
# UpdateConfigurationRecords.ps1
#
param(
[string]$CrmConnectionString,
[string]$EntityName,
[string]$LookupFieldName,
[string]$ValueFieldNames,
[string]$ConfigurationRecordsJson
)

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering UpdateConfigurationRecords.ps1'

#Parameters
Write-Verbose "EntityName = $EntityName"
Write-Verbose "LookupFieldName = $LookupFieldName"
Write-Verbose "ValueFieldNames = $ValueFieldNames"
Write-Verbose "ConfigurationRecordsJson = $ConfigurationRecordsJson"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

#Load XrmCIFramework
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.Cmdlets.dll"
Write-Verbose "Importing CIToolkit: $xrmCIToolkit"
Import-Module $xrmCIToolkit
Write-Verbose "Imported CIToolkit"

#load json string into array
$array = ConvertFrom-Json $ConfigurationRecordsJson

Write-Host ("Processing ({0}) records" -f $array.Count)

$valueFields = $ValueFieldNames.Split(",");

#iterate through the configuration items and set secure configuration
For ($i=0; $i -lt $array.Count; $i++)
{
	$lookup = $array[$i][0]

	Write-Host ("Processing record: {0}" -f $array[$i])

    $records = Get-XrmEntities -ConnectionString $CrmConnectionString -EntityName $EntityName -Attribute $LookupFieldName -Value $lookup -ConditionOperator 0

	if ($records.Count -eq 1)
	{
		
        $record = $records[0]
		$changed = $false

		For ($j=0; $j -lt $valueFields.Count; $j++)
		{
			$value =  $array[$i][$j+1]

			if ($record.Attributes[$valueFields[$j]] -cne $value)
			{
				$record.Attributes[$valueFields[$j]] = $value
				$changed = $true
			}
		}

		if ($changed)
		{
			Set-XrmEntity -ConnectionString $CrmConnectionString -EntityObject $record

			Write-Host ("Record Update Completed for Id: {0}" -f $record.Id)
		}
		else
		{
			Write-Host ("Record Update Skipped for Id: {0}" -f $record.Id)
		}
	}
	elseif ($records.Count -eq 0)
	{
		$record = New-XrmEntity -EntityName $EntityName
		$record.Attributes["$LookupFieldName"] = $lookup

		For ($j=0; $j -lt $valueFields.Count; $j++)
		{
			$value =  $array[$i][$j+1]
			$record.Attributes[$valueFields[$j]] = $value
		}

		$recordId = Add-XrmEntity -ConnectionString $CrmConnectionString -EntityObject $record

		Write-Host ("Record Create Completed for Id: {0}" -f $recordId)
	}
	else
	{
		Write-Error ("Multiple matches found for {0}" -f $lookup)
	}
}

Write-Host "Configuration Records Update Succeeded"

Write-Verbose 'Leaving UpdateConfigurationRecords.ps1'