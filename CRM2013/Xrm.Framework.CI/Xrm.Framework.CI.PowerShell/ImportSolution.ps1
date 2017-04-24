# Filename: ImportSolution.ps1

param(
[string]$solutionFile, #The full solution file name
[string]$solutionPath, #The full path to the location of the solution
[string]$targetCrmConnectionUrl, #The target CRM organization connection string
[bool]$override = $false, #If set to true will override the solution even if a solution with same version exists
[bool]$publishWorkflows = $false, #Will publish workflows during import
[bool]$overwriteUnmanagedCustomizations = $false, #Will overwrite unmanaged customizations
[bool]$skipProductUpdateDependencies = $false, #Will skip product update dependencies
[string]$logsDirectory #Optional - will place the import log in here
)

$ErrorActionPreference = "Stop"

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.dll"
Write-Output "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit

Write-Output "solutionFile: $solutionFile"
Write-Output "solutionPath: $solutionPath"
Write-Output "targetCrmConnectionUrl: $targetCrmConnectionUrl"
Write-Output "Override: $override"
Write-Output "Publish Workflows: $publishWorkflows"
Write-Output "overwrite Unmanaged Customizations: $overwriteUnmanagedCustomizations"
Write-Output "Skip Product Update Dependencies: $skipProductUpdateDependencies"
Write-Output "Logs Directory: $logsDirectory"

$solutionInfo = Get-XrmSolutionInfoFromZip -SolutionFilePath ($solutionPath + "\" + $solutionFile)

Write-Output "Solution to Import: " $solutionInfo.UniqueName
Write-Output "Solution to Import: " $solutionInfo.Version

$solution = Get-XrmSolution -ConnectionString $targetCrmConnectionUrl -UniqueSolutionName $solutionInfo.UniqueName

if ($solution -eq $null)
{
    Write-Output "Solution not currently installed"
}
else
{
    Write-Output "Solution Installed Current version: " $solution.Version
}
 
if ($override -or ($solution -eq $null) -or ($solution.Version -ne $solutionInfo.Version))
{
    $solutionImportPath = $solutionPath + "\" + $solutionFile
    
    Write-Output "Importing Solution: $solutionImportPath"

    $importJobId = [guid]::NewGuid()
  
    $asyncOperationId = Import-XrmSolution -ConnectionString $targetCrmConnectionUrl -SolutionFilePath $solutionImportPath -publishWorkflows $publishWorkflows -overwriteUnmanagedCustomizations $overwriteUnmanagedCustomizations -SkipProductUpdateDependencies $skipProductUpdateDependencies -ImportAsync $true -WaitForCompletion $true -ImportJobId $importJobId
 
    Write-Output "Solution Import Completed. Import Job Id: $importJobId"

    if ($logsDirectory)
    {
        $importLogFile = $logsDirectory + "\" + $solutionFile.Replace(".zip", "_" + [System.DateTime]::Now.ToString("yyyy_MM_dd__HH_mm") + ".xml")
    }

    $importJob = Get-XrmSolutionImportLog -ImportJobId $importJobId -ConnectionString $targetCrmConnectionUrl -OutputFile $importLogFile

    $importProgress = $importJob.Progress
    $importResult = (Select-Xml -Content $importJob.Data -XPath "//solutionManifest/result/@result").Node.Value
    $importErrorText = (Select-Xml -Content $importJob.Data -XPath "//solutionManifest/result/@errortext").Node.Value


    Write-Output "Import Progress: $importProgress"
    Write-Output "Import Result: $importResult"
    Write-Output "Import Error Text: $importErrorText"
    Write-Output $importJob.Data

    if (($importResult -ne "success") -or ($importProgress -ne 100))
    {
        throw "Import Failed"
    }

    $solution = Get-XrmSolution -ConnectionString $targetCrmConnectionUrl -UniqueSolutionName $solutionInfo.UniqueName

    if ($solution.Version -ne $solutionInfo.Version)
    {
        throw "Import Failed"
    }
    else
    {
        Write-Output "Solution Imported Successfully"
    }
}
else
{
    Write-Output "Skipped Import of Solution..."
}
