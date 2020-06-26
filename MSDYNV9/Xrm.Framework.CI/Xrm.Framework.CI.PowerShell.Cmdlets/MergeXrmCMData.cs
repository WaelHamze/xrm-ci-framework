using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// Performs Solution Packager-style mapping of VCS controlled artifacts on a VCS file structure generated from <see cref="ExpandXrmCmData"/>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Merge-XrmCmData -Folder "C:\VCSLocal\UnpackedRoot" -MappingFile "C:\VCSLocal\pathto\mappingfile\mappingfile.xml" </code>
    ///   <para>Folder: unpacked directory from <see cref="ExpandXrmCmData"/></para>
    ///   <para>MappingFile: a solution-packager style mapping xml for substituting in VCS controlled components at merge time (ie JS,CSS,html etc.)</para>
    ///   <para>MergeDataXmlFile: more granular control over level to which the data.xml file is merged. Options are Default (EntityLevel), None (do not perform any merging), EntityLevel (a file per entity, expecting \unpackedroot\(entityname).xml), RecordLevel (a file per record, expecting \UnpackedRoot\(entityname)\(recordid).xml. Should match the granular level of the <see cref="ExpandXrmCmData"/> call used to generate it</para>
    /// </example>
    [Cmdlet(VerbsData.Merge, "XrmCmData")]
    public class MergeXrmCmData : CommandBase
    {
        #region Parameters
        /// <summary>
        /// <para type="description">Path to the folder for the files to be merged</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Folder { get; set; }

        /// <summary>
        /// <para type="description">Path to the mapping file which describes the merges to be performed</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string MappingFile { get; set; }

        /// <summary>
        /// <para type="description">Determines the level to which the xml data is split in the folder structure</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateSet("Default", "None", "EntityLevel", "RecordLevel")]
        [PSDefaultValue(Value="Default")]
        public string MergeDataXmlFileLevel { get; set; }

        /// <summary>
        /// <para type="description">Determines whether to care about case when comparing target filenames to the mappingfile, Important for *Nix/Windows differing file structures</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public bool FileMapCaseSensitive { get; set; }
        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Logger.LogInformation("Merging data in folder {0} using mapping: {1}", Folder, MappingFile);

            ConfigurationMigrationManager manager = new ConfigurationMigrationManager(Logger);

            CmExpandTypeEnum mergeDataXmlFileLevelType = (CmExpandTypeEnum)Enum.Parse(typeof(CmExpandTypeEnum), MergeDataXmlFileLevel);

            manager.MapData(Folder, MappingFile, mergeDataXmlFileLevelType, FileMapCaseSensitive);

            Logger.LogInformation("Data Merge Completed");
        }

        #endregion
    }
}
