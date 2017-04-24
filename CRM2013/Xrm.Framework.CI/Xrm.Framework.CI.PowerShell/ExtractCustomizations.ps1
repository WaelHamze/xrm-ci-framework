# Filename: ExtractCustomizations.ps1
param([string]$solutionPackager, #The full path to the solutionpackager.exe
[string]$solutionFilesFolder, #The folder to extract the CRM solution
[string]$mappingFile, #The full path to the mapping file
[string]$solutionName, #The unique CRM solution name
[string]$connectionString, #The connection string as per CRM Sdk
[switch]$checkout, #Optional - pass if you want to Check Out the existing files in the extract location
[switch]$checkin) #Optional - pass if you want to Check In the updated files after extraction

$ErrorActionPreference = "Stop"

Write-Output "Solution Packager: $solutionPackager"
Write-Output "Solution Files Folder: $solutionFilesFolder"
Write-Output "Mapping File: $mappingFile"
Write-Output "ConnectionString: $connectionString"
Write-Output "Check In: $checkin"
Write-Output "Check Out: $checkout"

# CI Toolkit
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$xrmCIToolkit = $scriptPath + "\Xrm.Framework.CI.PowerShell.dll"
Write-Output "Importing CIToolkit: $xrmCIToolkit" 
Import-Module $xrmCIToolkit

#TF.exe
$tfCommand = "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\tf.exe"

#Check Pending Changes
$pendingChanges = & "$tfCommand" status /recursive /noprompt "$solutionFilesFolder"
if ($pendingChanges -like "*no pending changes*")
{
    Write-Output $pendingChanges
}
else
{
    Write-Output $pendingChanges
    Write-Error "Pending Changes Detected. Undo your changes and try again."
    return
}

#Before Files
[string[]]$beforeFiles = [System.IO.Directory]::GetFileSystemEntries($solutionFilesFolder, "*", [IO.SearchOption]::AllDirectories)
Write-Output "Before Files: " $beforeFiles

#Check Out
if ($checkout -and ($beforeFiles.Length -gt 0))
{
    & "$tfCommand" checkout /recursive /noprompt "$solutionFilesFolder"
}

#Export Solutions
Write-Output "Exporting Solutions to: " $env:TEMP
$unmanagedSolution = Export-XrmSolution -ConnectionString $connectionString -Managed $False -OutputFolder $env:TEMP -UniqueSolutionName $solutionName
Write-Output "Exported Solution: $unmanagedSolution"
$managedSolution = Export-XrmSolution -ConnectionString $connectionString -Managed $True -OutputFolder $env:TEMP -UniqueSolutionName $solutionName
Write-Output "Exported Solution: $managedSolution"

#Solution Packager
$extractOuput = & "$solutionPackager" /action:Extract /zipfile:"$env:TEMP\$unmanagedSolution" /folder:"$solutionFilesFolder" /packagetype:Both /errorlevel:Info /allowWrite:Yes /allowDelete:Yes /map:$mappingFile
Write-Output $extractOuput
if ($lastexitcode -ne 0)
{
    throw "Solution Extract operation failed with exit code: $lastexitcode"
}
else
{
    if (($extractOuput -ne $null) -and ($extractOuput -like "*warnings encountered*"))
    {
        Write-Warning "Solution Packager encountered warnings. Check the output."
    }
}

#After Files
[string[]]$afterFiles = [System.IO.Directory]::GetFileSystemEntries($solutionFilesFolder, "*", [IO.SearchOption]::AllDirectories)
Write-Output "After Files: " $afterFiles

#Get the deltas
$deletedFiles = $beforeFiles | where {$afterFiles -notcontains $_}
$addedFiles = $afterFiles | where {$beforeFiles -notcontains $_}
if ($deletedFiles.Length -gt 0)
{
    Write-Output "Deleted Files:" $deletedFiles
    
    $_files = [System.String]::Join(""" """, $deletedFiles)
    
    & "$tfCommand" undo /noprompt "$_files"
    & "$tfCommand" delete /noprompt "$_files"
}
if ($addedFiles.Length -gt 0)
{
    Write-Output "Added Files:" $addedFiles
    $_files = [System.String]::Join(""" """, $addedFiles)
    
    & "$tfCommand" add /noprompt "$_files"
}

# Checkin
if ($checkin)
{
    & "$tfCommand" checkin /noprompt /recursive /override:"PowerShell Extract" "$solutionFilesFolder" /bypass
}

# End of script