#
# ConnectionFunctions.ps1
#

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ConnectionFunctions.ps1'

function GetXrmConnectionFromConfig(
	[string]$key
	)
	{
		if ($key)
		{
			$connection = Get-XrmConnection -Key $key
		}
		else
		{
			$connections = Get-XrmConnections

			$count = $connection.length

			Write-Host "Stored Connections $count" -ForegroundColor Cyan

			For($i = 0; $i -lt $connections.length; $i++)
			{
				$key = $connections[$i];
				Write-Host "$i - $key" -ForegroundColor Yellow
			}

			$addIndex = $connections.length

			Write-Host "$addIndex - To add a new connection" -ForegroundColor Magenta

			$removeIndex = $addIndex + 1

			Write-Host "$removeIndex - To remove a connection" -ForegroundColor Magenta

			$choice = Read-Host "Enter number to the corresponding option"

			if (($choice -lt 0) -or ($choice -gt $removeIndex))
			{
				throw "Invalid choice"
			}

			if ($choice -eq $addIndex)
			{
				$key = Read-Host "Enter connection name"
				$connection = Read-Host "Enter connection string"

				Write-Host "Testing Connection"

				Select-WhoAmI -ConnectionString "$connection"

				Write-Host "Connection Tested"

				Set-XrmConnection -Key $key -ConnectionString $connection

				Write-Host "Connection Added" -ForegroundColor DarkGreen
			}
			elseif ($choice -eq $removeIndex)
			{
				Write-Host "Removing Connection"

				$choice = Read-Host "Enter number corresponding to connection you want to remove"

				$key = $connections[$choice]

				$confirm = Read-Host "You are removing connection with key $key. Enter Y to confirm or N to abort"

				if ($confirm -eq 'Y')
				{
					Remove-XrmConnection -Key $key
					Write-Host "Removed Connection" -ForegroundColor DarkGreen
				}
				else
				{
					Write-Host "Remove Aborted"
				}
			}
			else
			{
				$key = $connections[$choice]
				$connection = Get-XrmConnection -Key $key
			}
		}

		return $connection
	}
