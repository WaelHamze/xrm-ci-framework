﻿using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
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
        public String Path { get; set; }

        /// <summary>
        /// <para type="description">The unique name of the web resource in CRM. e.g. prefix_Test.js</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public String UniqueName { get; set; }

        /// <summary>
        /// <para type="description">The unique name of the web resource in CRM. e.g. prefix_Test.js</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public Boolean Publish { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose(string.Format("Updating Web Resource: {0}", Path));

            FileInfo webResourceInfo = new FileInfo(Path);

            String content = Convert.ToBase64String(File.ReadAllBytes(Path));

            Guid resourceId;

            using (var context = new CIContext(OrganizationService))
            {
                if (String.IsNullOrEmpty(UniqueName))
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
                        UniqueName = resources[0].Name;
                    }
                }
                else
                {
                    var query = from a in context.WebResourceSet
                                where a.Name == UniqueName
                                select a.Id;

                    resourceId = query.FirstOrDefault();

                    if (resourceId == null || resourceId == Guid.Empty)
                    {
                        throw new ItemNotFoundException(string.Format("{0} was not found", UniqueName));
                    }
                }

                WebResource webResource = new WebResource()
                {
                    Id = resourceId,
                    Content = content
                };

                OrganizationService.Update(webResource);

                WriteObject(resourceId);

                base.WriteVerbose(string.Format("Web Resource Updated Name: {0}", UniqueName));

                if (Publish)
                {
                    base.WriteVerbose(string.Format("Publishing Web Resource: {0}", UniqueName));

                    PublishXmlRequest req = new PublishXmlRequest()
                    {
                        ParameterXml = string.Format("<importexportxml><webresources><webresource>{0}</webresource></webresources></importexportxml>", resourceId)
                    };

                    OrganizationService.Execute(req);

                    base.WriteVerbose(string.Format("Published Web Resource: {0}", UniqueName));
                }
            }
        }

        #endregion
    }
}
