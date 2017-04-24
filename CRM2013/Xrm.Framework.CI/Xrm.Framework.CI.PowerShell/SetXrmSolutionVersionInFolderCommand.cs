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
    [Cmdlet(VerbsCommon.Set, "XrmSolutionVersionInFolder")]
    public class SetXrmSolutionVersionInFolderCommand : Cmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true)]
        public string SolutionFilesFolderPath
        {
            get { return solutionFilesFolderPath; }
            set { solutionFilesFolderPath = value; }
        }
        private string solutionFilesFolderPath;

        [Parameter(Mandatory = true)]
        public string Version
        {
            get { return version; }
            set { version = value; }
        }
        private string version;

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string uniqueName;

            string solutionFile = SolutionFilesFolderPath + "\\Other\\Solution.xml";

            base.WriteVerbose(string.Format("Reading Solution File: {0}", solutionFile));

            XElement solutionNode;
            
            using (StreamReader reader = new StreamReader(solutionFile))
            {
                solutionNode = XElement.Load(reader);
                uniqueName = solutionNode.Descendants("UniqueName").First<XElement>().Value;

                base.WriteVerbose(string.Format("Updating Version for Solution: {0}", uniqueName));

                solutionNode.Descendants("Version").First<XElement>().Value = Version;
            }

            solutionNode.Save(solutionFile);
        }

        #endregion
    }
}
