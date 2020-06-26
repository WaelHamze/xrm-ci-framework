using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Compresses a merged folder from <see cref="ExpandXrmCmData"/></para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Compress-XrmCmData -DataZip "C:\pathto\outputzipfile\data.zip" -Folder "C:\pathto\unpackedfolder\tocompress"</code>
    /// </example>
    [Cmdlet(VerbsData.Compress, "XrmCmData")]
    public class CompressXrmCmData : CommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute output path to the data zip file to be generated</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string DataZip { get; set; }

        /// <para type="description">The source folder of the extracted files</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Folder { get; set; }

        /// <summary>
        /// <para type="description">More granular control over level to which the data.xml file is merged. Options are Default (EntityLevel), None (do not perform any merging), EntityLevel (a file per entity, expecting \unpackedroot\(entityname).xml), RecordLevel (a file per record, expecting \UnpackedRoot\(entityname)\(recordid).xml. Should match the granular level of the <see cref="ExpandXrmCmData"/> call used to generate it</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateSet("Default", "None", "EntityLevel", "RecordLevel")]
        [PSDefaultValue(Value = "Default")]
        public string CombineDataXmlFileLevel { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogInformation("Compressing folder {0} to file: {0}", DataZip, Folder);

            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(Logger);

            var combineDataXmlFileLevelType = (CmExpandTypeEnum)Enum.Parse(typeof(CmExpandTypeEnum), CombineDataXmlFileLevel);

            if (combineDataXmlFileLevelType != CmExpandTypeEnum.None)
            {
                string tempDirectory = manager.CombineData(Folder, combineDataXmlFileLevelType);
                manager.CompressData(tempDirectory, DataZip);
            }
            else
            {
                manager.CompressData(Folder, DataZip);
            }

            Logger.LogInformation("Compression Completed");
        }

        #endregion
    }
}