using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.IO;
using Xrm.Framework.CI.Common.Entities;
using Microsoft.Xrm.Sdk.Messages;
using System.IO.Compression;
using Xrm.Framework.CI.Common;
using System.Xml.Linq;

namespace Xrm.Framework.CI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "XrmSolutionInfoFromZip")]
    public class GetXrmSolutionInfoFromZipCommand : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string SolutionFilePath
        {
            get { return solutionFilePath; }
            set { solutionFilePath = value; }
        }
        private string solutionFilePath;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Reading Solution Zip: {0}", SolutionFilePath));

            string uniqueName;
            string version;

            using (ZipArchive solutionZip = ZipFile.Open(solutionFilePath, ZipArchiveMode.Read))
            {
                    ZipArchiveEntry solutionEntry = solutionZip.GetEntry("solution.xml");

                    using (StreamReader reader = new StreamReader(solutionEntry.Open()))
                    {
                        XElement solutionNode = XElement.Load(reader);
                        uniqueName = solutionNode.Descendants("UniqueName").First<XElement>().Value;
                        version = solutionNode.Descendants("Version").First<XElement>().Value;
                    }
            }

            XrmSolutionInfo info = new XrmSolutionInfo()
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
