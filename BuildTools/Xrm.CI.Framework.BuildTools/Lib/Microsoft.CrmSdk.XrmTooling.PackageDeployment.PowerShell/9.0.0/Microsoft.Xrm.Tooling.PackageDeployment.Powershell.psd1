#
# Module manifest for module 'Microsoft.Xrm.Tooling.CrmConnector.PowerShell'
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'Microsoft.Xrm.Tooling.PackageDeployment.Powershell.psm1'

# Version number of this module.
ModuleVersion = '9.0.0'

# ID used to uniquely identify this module
GUID = 'C46F26FB-42AB-4226-BF3F-868DE04AE14C'

# Author of this module
Author = 'Microsoft Dynamics© CRM'

# Company or vendor of this module
CompanyName = 'Microsoft'

# Copyright statement for this module
Copyright = 'Copyright © 2017, . All rights reserved.'

# Description of the functionality provided by this module
Description = 'PowerShell Module for XRM Package Deployer'

# Minimum version of the .NET Framework required by this module
DotNetFrameworkVersion = '4.5.2'

# Minimum version of the common language runtime (CLR) required by this module
CLRVersion = '4.0'

# Assemblies that must be loaded prior to importing this module
RequiredAssemblies = @(
'Microsoft.Xrm.Tooling.Ui.Styles.dll',
'Microsoft.Uii.Common.dll',
'Microsoft.Uii.AifServices.dll',
'Microsoft.Uii.Common.TypeProvider.dll',
'Microsoft.Uii.CrmEntityManager.dll'
)

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
NestedModules = @(
	'Microsoft.Xrm.Tooling.PackageDeployment.Powershell.dll'
	)

# Functions to export from this module
FunctionsToExport = @()

# Cmdlets to export from this module
CmdletsToExport = @(
	'Get-CrmPackages',
    'Import-CrmPackage'
	)

# Variables to export from this module
VariablesToExport = @(
	)

# Aliases to export from this module
AliasesToExport = @(
	)

# List of all modules packaged with this module.
 ModuleList = @(
	'Microsoft.Xrm.Tooling.PackageDeployment.Powershell'
	)


}