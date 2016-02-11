using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Xrm.Framework.CI.PowerShell.Command
{
    /// <summary>
    /// <para type="synopsis">Returns solution info from a solution zip file</para>
    /// <para type="description">This cmdlet returns the unique solution name and version number
    ///  from a solution zip file
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmSolutionInfoFromZip")]
    [OutputType(typeof(XrmSolutionInfo))]
    public class GetXrmSolutionInfoFromZipCommand : Cmdlet
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to where the solution zip file</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionFilePath { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Reading Solution Zip: {0}", SolutionFilePath));

            string uniqueName;
            string version;

            using (ZipArchive solutionZip = ZipFile.Open(SolutionFilePath, ZipArchiveMode.Read))
            {
                ZipArchiveEntry solutionEntry = solutionZip.GetEntry("solution.xml");

                using (var reader = new StreamReader(solutionEntry.Open()))
                {
                    XElement solutionNode = XElement.Load(reader);
                    uniqueName = solutionNode.Descendants("UniqueName").First().Value;
                    version = solutionNode.Descendants("Version").First().Value;
                }
            }

            var info = new XrmSolutionInfo
            {
                UniqueName = uniqueName,
                Version = version
            };

            base.WriteObject(info);
        }

        #endregion
    }

    public class XrmSolutionInfo
    {
        public string UniqueName { get; set; }
        public string Version { get; set; }
    }
}
