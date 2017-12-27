#
# DeployTask.ps1
#

param(
[string]$Url,
[string]$token
)

$ErrorActionPreference = "Stop"

tfx login --service-url $url --auth-type 'pat' --token $token

$tasks = Get-ChildItem .\Tasks -directory
$counter = 1

foreach($task in $tasks)
{
    Write-Host ('' + $counter + ' - ' + $task)
    $counter++
}

$index = Read-Host -Prompt "Enter Task #"

$selectedTask = $($tasks[$index -1])
Write-Host "Deploying Task $selectedTask" -ForegroundColor Cyan

$taskMetadata = Get-Content -Raw -Path .\Tasks\$selectedTask\task.json | ConvertFrom-Json

#$taskMetadata.version.Patch = '' + ([int]$taskMetadata.version.Patch + 1)

#$taskMetadata | ConvertTo-Json | Format-Json | Out-File .\Tasks\$selectedTask\task.json

$newVersion = ([int]$taskMetadata.version.Patch + 1)

(Get-Content .\Tasks\$selectedTask\task.json).replace('"Patch": "' + $taskMetadata.version.Patch + '"', '"Patch": "' + $newVersion + '"') | Set-Content .\Tasks\$selectedTask\task.json

Write-Host ("Version: " + $taskMetadata.version.Major + "." + $taskMetadata.version.Minor + "." + $newVersion) -ForegroundColor Yellow

.\Make.ps1

tfx build tasks upload --task-path .\bin\Tasks\$selectedTask

Write-Host "Deployment Completed" -ForegroundColor Green