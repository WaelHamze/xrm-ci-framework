#
# Xrm.Framework.CI.PowerShell.psm1
#

[CmdletBinding()]


$ErrorActionPreference = "Stop"

#Script Location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
Write-Verbose "Script Path: $scriptPath"

."$scriptPath\ConnectionFunctions.ps1"
