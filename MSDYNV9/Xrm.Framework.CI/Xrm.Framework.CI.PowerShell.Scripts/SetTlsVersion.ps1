#
# SetTlsVersion.ps1
#

$currentProtocol = [System.Net.ServicePointManager]::SecurityProtocol
Write-Verbose "Current Security Protocol: $currentProtocol"
if (-not $currentProtocol.HasFlag([System.Net.SecurityProtocolType]::Tls11))
{
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol  -bor [System.Net.SecurityProtocolType]::Tls11;
}
if (-not $currentProtocol.HasFlag([System.Net.SecurityProtocolType]::Tls12))
{
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol  -bor [System.Net.SecurityProtocolType]::Tls12;
}
$currentProtocol = [System.Net.ServicePointManager]::SecurityProtocol
Write-Verbose "Modified Security Protocol: $currentProtocol"
