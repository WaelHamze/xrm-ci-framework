using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Xml;
using System.Xml.Serialization;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Exports all access team templates from a CRM organization to an XML file.</para>
    /// <para type="description">Export-XrmAccessTeamTemplates exports all access team templates from a CRM organization to an XML file.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmAccessTeamTemplates -ConnectionString "" -OutputPath "FULL PATH TO EXPORT FILE"</code>
    ///   <para>Exports teamteplate reords to outputpath location</para>
    /// </example>
    [Cmdlet(VerbsData.Export, "XrmAccessTeamTemplates")]
    [OutputType(typeof(String))]
    public class ExportXrmAccessTeamTemplatesCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the location of the file containing the exported access team templates</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string OutputPath { get; set; }

        public ExportXrmAccessTeamTemplatesCommand()
        {
        }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Exporting access team templates to: {0}", OutputPath));

            string teamtemplateFetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>  
              <entity name='teamtemplate'>
                <attribute name='defaultaccessrightsmask' />
                <attribute name='description' />
                <attribute name='objecttypecode' />
                <attribute name='teamtemplateid' />
                <attribute name='teamtemplatename' />
              </entity>
            </fetch>";

            var retrievedRecords = OrganizationService.RetrieveMultiple(new FetchExpression(teamtemplateFetch));

            List<ExportEntity> exportEntities = new List<ExportEntity>();
            if (retrievedRecords.Entities.Count > 0)
            {
                foreach (Entity entity in retrievedRecords.Entities)
                {
                    ExportEntity exportEntity = new ExportEntity();
                    exportEntity.LogicalName = entity.LogicalName;
                    exportEntity.Id = entity.Id;
                    exportEntity.Attributes = new List<SerializableKeyValuePair<string, object>>();
                    exportEntity.FormattedValues = new List<SerializableKeyValuePair<string, string>>();
                    foreach (var attribute in entity.Attributes)
                    {
                        SerializableKeyValuePair<string, object> serializedAttribute = new SerializableKeyValuePair<string, object>();
                        serializedAttribute.Key = attribute.Key;
                        serializedAttribute.Value = attribute.Value;
                        exportEntity.Attributes.Add(serializedAttribute);
                    }
                    foreach (var attribute in entity.FormattedValues)
                    {
                        SerializableKeyValuePair<string, string> serializedAttribute = new SerializableKeyValuePair<string, string>();
                        serializedAttribute.Key = attribute.Key;
                        serializedAttribute.Value = attribute.Value;
                        exportEntity.FormattedValues.Add(serializedAttribute);
                    }
                    exportEntities.Add(exportEntity);
                }
            }
            ExportedTeamTemplates templates = new ExportedTeamTemplates();
            templates.TeamTemplates = exportEntities;
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(templates.GetType());
            TextWriter templateWriter = new StreamWriter(OutputPath);

            x.Serialize(templateWriter, templates);

            base.WriteObject("Export complete");
        }

        #endregion
    }

    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {

        public SerializableKeyValuePair()
        {
        }

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }

    }

    [Serializable]
    public class ExportEntity
    {
        public string LogicalName { get; set; }
        public Guid Id { get; set; }
        public System.Collections.Generic.List<SerializableKeyValuePair<string, object>> Attributes { get; set; }
        public System.Collections.Generic.List<SerializableKeyValuePair<string, string>> FormattedValues { get; set; }
    }

    [Serializable]
    public class ExportedTeamTemplates
    {
        public List<ExportEntity> TeamTemplates { get; set; }
    }
}