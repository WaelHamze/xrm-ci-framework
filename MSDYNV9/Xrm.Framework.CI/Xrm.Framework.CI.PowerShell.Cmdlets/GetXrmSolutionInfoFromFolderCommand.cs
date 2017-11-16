using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Returns solution info from an extracted solution zip file</para>
    /// <para type="description">This cmdlet returns the unique solution name and version number
    ///  from an unpacked solution folder by the solutionpackager.exe
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Export-XrmSolution -ConnectionString "" -EntityName "account"</code>
    ///   <para>Exports the "" managed solution to "" location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "XrmSolutionInfoFromFolder")]
    [OutputType(typeof(XrmSolutionInfo))]
    public class GetXrmSolutionInfoFromFolderCommand : Cmdlet
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to where the solution zip was unpacked to using solutionpackager.exe</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SolutionFilesFolderPath { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Reading Solution Files Folder: {0}", SolutionFilesFolderPath));

            string uniqueName;
            string version;

            using (var reader = new StreamReader(SolutionFilesFolderPath + "\\Other\\Solution.xml"))
            {
                XElement solutionNode = XElement.Load(reader);
                uniqueName = solutionNode.Descendants("UniqueName").First().Value;
                version = solutionNode.Descendants("Version").First().Value;
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
}
