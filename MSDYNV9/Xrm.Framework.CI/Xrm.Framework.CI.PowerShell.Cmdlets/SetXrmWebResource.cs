using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates a Web Resource.</para>
    /// <para type="description">The Set-WebResource cmdlet updates an existing Web Resource in CRM.
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-WebResource -Path $path</code>
    ///   <para>Updates a Web Resource.</para>
    /// </example>
    /// <para type="link" uri="http://msdn.microsoft.com/en-us/library/microsoft.xrm.sdk.messages.updaterequest.aspx">UpdateRequest.</para>
    [Cmdlet(VerbsCommon.Set, "XrmWebResource")]
    public class SetXrmWebResource : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The full path to the web resource file. e.g. C:\Solution\WebResources\Test.js</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public String WebResourceProjectPath { get; set; }

        /// <summary>
        /// <para type="description">The unique name of the web resource in CRM. e.g. prefix_Test.js</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public String Publish { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Updating Web Resources from : {0}", WebResourceProjectPath));

            bool publish = Convert.ToBoolean(Publish);

            string xmlFile = File.ReadAllText(WebResourceProjectPath);
            string dirPath = System.IO.Path.GetDirectoryName(WebResourceProjectPath);
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlFile);
            XmlNodeList nodeList = xmldoc.GetElementsByTagName("CRMWebResource");
            foreach (XmlNode node in nodeList)
            {
                string path = dirPath + "\\" + node.Attributes["Include"].Value;
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name.Equals("UniqueName"))
                    {
                        string uniqueName = childNode.InnerText;
                        base.WriteVerbose(string.Format("Updating Web Resource : {0}", path));
                        this.UpdateWebResource(publish, path, uniqueName);
                    }
                }
            }
        }

        private void UpdateWebResource(bool publish, string path, string uniqueName)
        {
            FileInfo webResourceInfo = new FileInfo(path);

            String content = Convert.ToBase64String(File.ReadAllBytes(Uri.UnescapeDataString(path)));

            Guid resourceId;

            using (var context = new CIContext(OrganizationService))
            {
                if (String.IsNullOrEmpty(uniqueName))
                {
                    var query = from a in context.WebResourceSet
                                where a.Name.Contains(webResourceInfo.Name)
                                select new WebResource
                                {
                                    Name = a.Name,
                                    Id = a.Id
                                };

                    var resources = query.ToList<WebResource>();

                    if (resources.Count != 1)
                    {
                        throw new ItemNotFoundException(string.Format("{0} web resources found matching file name", resources.Count));
                    }
                    else
                    {
                        resourceId = resources[0].Id;
                        uniqueName = resources[0].Name;
                    }
                }
                else
                {
                    var query = from a in context.WebResourceSet
                                where a.Name == uniqueName
                                select a.Id;

                    resourceId = query.FirstOrDefault();

                    if (resourceId == null || resourceId == Guid.Empty)
                    {
                        throw new ItemNotFoundException(string.Format("{0} was not found", uniqueName));
                    }
                }

                WebResource webResource = new WebResource()
                {
                    Id = resourceId,
                    Content = content
                };

                OrganizationService.Update(webResource);

                WriteObject(resourceId);

                base.WriteVerbose(string.Format("Web Resource Updated Name: {0}", uniqueName));

                if (publish)
                {
                    base.WriteVerbose(string.Format("Publishing Web Resource: {0}", uniqueName));

                    PublishXmlRequest req = new PublishXmlRequest()
                    {
                        ParameterXml = string.Format("<importexportxml><webresources><webresource>{0}</webresource></webresources></importexportxml>", resourceId)
                    };

                    OrganizationService.Execute(req);

                    base.WriteVerbose(string.Format("Published Web Resource: {0}", uniqueName));
                }
            }
        }

        #endregion
    }
}
