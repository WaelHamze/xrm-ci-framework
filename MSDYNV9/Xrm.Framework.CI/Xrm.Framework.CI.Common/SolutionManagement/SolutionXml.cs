using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class SolutionXml
    {
        protected ILogger Logger
        {
            get;
            set;
        }

        public SolutionXml(
            ILogger logger)
        {
            Logger = logger;
        }

        public XrmSolutionInfo GetSolutionInfoFromZip(
            string zipFile)
        {
            Logger.LogVerbose("Reading Solution Zip: {0}", zipFile);

            try
            {
                using (ZipArchive solutionZip = ZipFile.Open(zipFile, ZipArchiveMode.Read))
                {
                    var solutionEntry = solutionZip.Entries.First(e => e.Name.ToLower() == "solution.xml");

                    using (var reader = new StreamReader(solutionEntry.Open()))
                    {
                        return GetXrmSolutionInfoFromStream(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public XrmSolutionInfo GetXrmSolutionInfoFromFolder(
            string folder)
        {
            try
            {
                using (var reader = new StreamReader($"{folder}\\Other\\Solution.xml"))
                {
                    return GetXrmSolutionInfoFromStream(reader);
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        public void UpdateSolutionVersion(string folder, string version)
        {
            string solutionFile = folder + "\\Other\\Solution.xml";

            Logger.LogVerbose("Reading Solution File: {0}", solutionFile);

            XElement solutionNode;

            using (var reader = new StreamReader(solutionFile))
            {
                solutionNode = XElement.Load(reader);
                string uniqueName = solutionNode.Descendants("UniqueName").First().Value;

                Logger.LogVerbose(string.Format("Updating Version for Solution: {0}", uniqueName));

                solutionNode.Descendants("Version").First().Value = version;
            }

            solutionNode.Save(solutionFile);

            Logger.LogInformation("Version Updated to: {0}", version);
        }

        private XrmSolutionInfo GetXrmSolutionInfoFromStream(StreamReader reader)
        {
            XrmSolutionInfo info = null;

            try
            {
                string uniqueName;
                string version;

                XElement solutionNode = XElement.Load(reader);
                uniqueName = solutionNode.Descendants("UniqueName").First().Value;
                version = solutionNode.Descendants("Version").First().Value;

                info = new XrmSolutionInfo
                {
                    UniqueName = uniqueName,
                    Version = version
                };
            }
            catch(Exception ex)
            {
                Logger.LogError(ex.Message);
            }

            return info;
        }
    }
}
