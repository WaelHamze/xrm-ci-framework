#
# ConnectionFunctions.ps1
#

$ErrorActionPreference = "Stop"

Write-Verbose 'Entering ConnectionFunctions.ps1'

function GetXrmConnectionFromUser(
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

			$count = $connections.length

			Write-Host "-----------------------------------------------------"
			Write-Host "xRMCIFramework Connection Management" -ForegroundColor Cyan
			Write-Host "Existing stored connections are in Yellow"
			Write-Host "Additional options are in Magenta"
			Write-Host "-----------------------------------------------------"

			For($i = 0; $i -lt $connections.length; $i++)
			{
				$key = $connections[$i];
				Write-Host "$i - $key" -ForegroundColor Yellow
			}

			$addIndex = $connections.length
			Write-Host "$addIndex - To add a new connection" -ForegroundColor Magenta

			$removeIndex = $addIndex + 1
			Write-Host "$removeIndex - To remove a connection" -ForegroundColor Magenta

			$exitIndex = $removeIndex + 1
			Write-Host "$exitIndex - To Exit" -ForegroundColor Magenta

			Write-Host "-----------------------------------------------------"

			$choice = Read-Host "Enter number to the corresponding option"

			if (($choice -lt 0) -or ($choice -gt $exitIndex))
			{
				Write-Host "Invalid choice, enter a number between 0 and $exitIndex" -ForegroundColor Red
				$any = Read-Host "Press any key to continue"
			}
			elseif ($choice -eq $addIndex)
			{
				$key = Read-Host "Enter connection name"
				$connection = Read-Host "Enter connection string"

				Write-Host "Testing Connection"

				Select-WhoAmI -ConnectionString "$connection"

				Write-Host "Connection Tested"

				Set-XrmConnection -Key $key -ConnectionString $connection

				Write-Host "Connection Added" -ForegroundColor DarkGreen

				$any = Read-Host "Press any key to continue"
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
					$any = Read-Host "Press any key to continue"
				}
			}
			elseif($choice -eq $exitIndex)
			{
				$confirm = Read-Host "Are you sure you want to exit? $key. Enter Y to confirm or N to abort"

				if ($confirm -eq 'Y')
				{
					exit
				}
				else
				{
					Write-Host "Exit Aborted"
					$any = Read-Host "Press any key to continue"
				}
			}
			else
			{
				$key = $connections[$choice]
				$connection = Get-XrmConnection -Key $key

				Write-Host "Using Connection: $key"
			}
		}

		return $connection
	}

	function GetXrmConnectionFromConfig(
		[string]$key)
	{
		$connection = GetXrmConnectionFromUser($key)

		while (-not $connection)
		{
			$connection = GetXrmConnectionFromUser($key)
		}

		return $connection
	}

