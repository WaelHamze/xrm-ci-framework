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
    [Cmdlet(VerbsCommon.Get, "XrmSolutionInfoFromFolder")]
    public class GetXrmSolutionInfoFromFolderCommand : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string SolutionFilesFolderPath
        {
            get { return solutionFilesFolderPath; }
            set { solutionFilesFolderPath = value; }
        }
        private string solutionFilesFolderPath;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Reading Solution Files Folder: {0}", SolutionFilesFolderPath));

            string uniqueName;
            string version;

            using (StreamReader reader = new StreamReader(SolutionFilesFolderPath + "\\Other\\Solution.xml"))
            {
                XElement solutionNode = XElement.Load(reader);
                uniqueName = solutionNode.Descendants("UniqueName").First<XElement>().Value;
                version = solutionNode.Descendants("Version").First<XElement>().Value;
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
}
