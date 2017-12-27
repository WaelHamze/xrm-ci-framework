# xRM CI Framework
**The xRM Continuous Integration (CI) Framework is a set of tools that makes it easy and quick to automate builds and deployment of your CRM components.**

This will allow you to setup a fully automated DevOps pipeline so you can deliver CRM more frequently in a consistent and reliable way.

The latest version of all tasks work on the VSTS Hosted Agent.

## Supported Versions

**Dynamics 365 (8.x.x)** (Some tasks may work with previous version of CRM)
**VSTS/TFS** For support and installation [instructions](https://docs.microsoft.com/en-us/vsts/marketplace/get-tfs-extensions)

## Task Catalog

Below is a list of tasks that is included with the xRM CI Framework

| Task | Description |
| --- | --- |
| **MSCRM Ping** | A sample task that checks connectivity to a Dynamics 365 environment |
| **MSCRM Export Solution** | Exports a CRM Solution from the source CRM environment |
| **MSCRM Publish Customizations** | Publishes all CRM customizations |
| **MSCRM Set Version** | Updates the version of a CRM Solution |
| **MSCRM Pack Solution** | Packages a CRM Solution using SolutionPackager.exe |
| **MSCRM Import** | Import a Dynamics CRM Solution package |
| **MSCRM Package Deployer** | Deploys a CRM Package using the CRM Package Deployer PowerShell Cmdlets |
| **MSCRM Update Secure Configuration** | A task that updates Dynamics 365 plugin secure configuration |
| **MSCRM Get Online Instance By Name** | Gets an Online instance ID based on the name of the instance |
| **MSCRM Backup Online Instance** | Creates a backup of a Dynamics 365 Customer Engagement Online Instance |
| **MSCRM Provision Online Instance** | Creates a new Dynamics 365 Customer Engagement Online Instance |
| **MSCRM Restore Online Instance** | Restores an online instance from a previous backup |
| **MSCRM Delete Online Instance** | Deletes an Online Instance |
| **MSCRM Set Online Instance Admin Mode** | Enable/Disable administration mode on Online Instances |

## Build Automation

You can combine the xRM CI Framework tasks with other tasks to create a build definition as needed.

Below is a sample build definition that publishes customizations, updates solution version to match build number, exports the solution from CRM and then publishes the zip as an artifact in VSTS

![Build Definition](Images/OnlineBuildDefinition.png)

![Build Console](Images/OnlineBuildConsole.png)

![Build Artifacts](Images/OnlineBuildArtifacts.png)

## Release Automation

You can combine the xRM CI Framework tasks with other tasks to create a release definition as needed.

Below is a sample release definition that imports the solution generated from the latest build into the QA environment.

![Release Definition](Images/ThirdPartyReleaseDefinition.png)

![Release Logs](Images/ThirdPartyReleaseLogs.png)

![Solution Imported](Images/ThirdPartySolutionImported.png)

## More Information

For more documentation and source code, check out Github using the links on this page.

## Version History

**8.0.x**
Initial Release

**8.1.x**
Added task to backup CRM online instances

**8.2.x**
MSCRM Backup Online Instance now uses instance name instead of instance id
Added Tasks for Provision, Restore, Delete and Get Online Instances
Added Task for updating Secure Configuration of Plug-ins

**8.3.x**
Added Task Set Online Instance Admin Mode

For more information on changes between versions, check the commits on GitHub
