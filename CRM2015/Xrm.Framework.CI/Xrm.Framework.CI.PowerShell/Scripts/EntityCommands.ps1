$cs = "Url=http://mycrm.com/myorg"

Import-Module "..\Xrm.Framework.CI.PowerShell.dll"

Write-Host "Testing Entity Operations"

Write-Host "Create new Entity object with firstname 'firstname'"
$entity = New-XrmEntity -EntityName "contact"
$entity.Attributes.Add("firstname", "firstname")
Write-Host "Done" -ForegroundColor Green
Write-Host

Write-Host "Save the entity to crm"
$newEntityId = Add-XrmEntity -ConnectionString $cs -EntityObject $entity 
Write-Host "Done" -ForegroundColor Green
Write-Host

Write-Host "Get the created entity by its id"
$newEntity = Get-XrmEntity -ConnectionString $cs -Id $newEntityId -EntityName "contact"
Write-Host "Done" -ForegroundColor Green
Write-Host

Write-Host "Update the lastname property with 'lastname' and save to crm"
$newEntity.LastName = "lastname"
Set-XrmEntity -ConnectionString $cs -EntityObject $newEntity
Write-Host "Done" -ForegroundColor Green
Write-Host

Write-Host "Get entities by filter the attribute 'lastname'"
$entities = Get-XrmEntities -ConnectionString $cs -EntityName "contact" -Attribute "lastname" -Value "lastname"
Write-Host "Found " $entities.Count
Write-Host "Done" -ForegroundColor Green
Write-Host

Write-Host "Delete the entities"
 foreach ($e in $entities) {
    Remove-XrmEntity -ConnectionString $cs -Id $e.Id -EntityName "contact"
}

 Write-Host "Done" -ForegroundColor Green