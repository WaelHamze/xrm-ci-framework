Import-Module "..\Xrm.Framework.CI.PowerShell.dll"

# Specify a crm connectionstring.
# See here: http://msdn.microsoft.com/en-us/library/gg695810.aspx
# Required.
$connectionString = "Url=https://crm.crm2013.is-lab.ch/msa;"

# If set to false the job completion will be awaited.
# we will monitor the job by our selfe so it should be executed async.
# Not required. Default is false.
$async = $true

# Delete all Accounts
# Required.
[string[]]$querySet = @("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>   <entity name='account'>   </entity> </fetch>")

# Start the job in 5 minutes.
# Not required. Default is DateTime.Now.
$startTime = Get-Date
$startTime = $startTime.AddMinutes(5)

# The name for the operation which will show up in the UI.
# Not required. Default is "Bulkdelete - DateTime.Now".
$bulkDeleteOperationName = "test"

# Specify a recurrence pattern.
# See here: http://msdn.microsoft.com/en-us/library/cc189846.aspx
# Not required. Default is empty (none recurrence).
$recurrencePattern = "FREQ=DAILY;INTERVAL=1;"

# Specify whether a email notification should be sended after completion.
# Not required. Default is "false".
$sendEmailNotification = $false

# Specify the receiver of the email notification (only used if SendEmailNotification is true).
# The passed strings must be valid Guids which represents the Ids of the SystemUsers who should receive the email.
# Not required. Default is empty.
$toRecipients = @("")

# See ToReceipients.
# Not required. Default is empty.
$ccRecipients = @("")

# Create the BulkDeleteJob. The Id of the AsyncOperation will be retunred.
$jobId = New-XrmBulkDeleteJob -ConnectionString $connectionString -QuerySet $querySet -Async $async -StartTime $startTime -BulkDeleteOperationName $bulkDeleteOperationName -RecurrencePattern $recurrencePattern -SendEmailNotification $sendEmailNotification

# Use the retunred Id to get the AsyncOperation
$asyncOperation = Get-XrmEntity -ConnectionString $connectionString -EntityName "asyncoperation" -EntityId $jobId
$asyncOperation.StatusCode.Value

Read-Host