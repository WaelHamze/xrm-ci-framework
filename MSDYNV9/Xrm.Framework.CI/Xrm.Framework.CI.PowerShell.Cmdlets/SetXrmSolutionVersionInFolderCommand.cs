using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates the version number of an unpacked CRM solution</para>
    /// <para type="description">This cmdlet updates the version number of a solution that was unpacked using the solutionpackager.exe
    /// Ensure \\Other\\Solution.xml is not read-only
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmSolutionVersionInFolder")]
    public class SetXrmSolutionVersionInFolderCommand : Cmdlet
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to where the solution zip was unpacked to using solutionpackager.exe</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionFilesFolderPath { get; set; }

        /// <summary>
        /// <para type="description">The new version number of the solution</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Version { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string solutionFile = SolutionFilesFolderPath + "\\Other\\Solution.xml";

            base.WriteVerbose(string.Format("Reading Solution File: {0}", solutionFile));

            XElement solutionNode;

            using (var reader = new StreamReader(solutionFile))
            {
                solutionNode = XElement.Load(reader);
                string uniqueName = solutionNode.Descendants("UniqueName").First().Value;

                base.WriteVerbose(string.Format("Updating Version for Solution: {0}", uniqueName));

                solutionNode.Descendants("Version").First().Value = Version;
            }

            solutionNode.Save(solutionFile);
        }

        #endregion
    }
}