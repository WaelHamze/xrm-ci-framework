using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Imports all access team templates from an export file into a CRM organization.</para>
    /// <para type="description">Import-XrmAccessTeamTemplates imports all access team templates from an export file into a CRM organization.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Import-XrmAccessTeamTemplates -ConnectionString "" -InputPath "FULL PATH TO EXPORT FILE"</code>
    ///   <para>Imports teamteplate reords from inputpath location</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "XrmAccessTeamTemplates")]
    [OutputType(typeof(String))]
    public class ImportXrmAccessTeamTemplatesCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the location of the file containing the exported access team templates</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string InputPath { get; set; }

        public ImportXrmAccessTeamTemplatesCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Importing access team templates from: {0}", InputPath));
            ExportedTeamTemplates templates = new ExportedTeamTemplates();
            var serializer = new XmlSerializer(templates.GetType());
            using (TextReader reader = new StreamReader(InputPath))
            {
                templates = (ExportedTeamTemplates)serializer.Deserialize(reader);
            }

            foreach (ExportEntity exportedEntity in templates.TeamTemplates)
            {
                Entity entity = new Entity(exportedEntity.LogicalName);
                entity.Id = exportedEntity.Id;
                foreach(var attribute in exportedEntity.Attributes)
                {
                    entity[attribute.Key] = attribute.Value;
                }
                try
                {
                    //first try an update
                    OrganizationService.Update(entity);
                }
                catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
                {
                    //if update fails, try a create
                    OrganizationService.Create(entity);
                }
            }

            base.WriteObject("Import complete");
        }

        #endregion
    }
}