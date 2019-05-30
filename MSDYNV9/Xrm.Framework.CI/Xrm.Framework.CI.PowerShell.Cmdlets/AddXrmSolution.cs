using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new unmanaged CRM Solution</para>
    /// <para type="description">Creates a new unmanaged CRM Solution</para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Add-XrmSolution -CrmConnectionString $crmConnectionString -UniqueName $UniqueName -DisplayName "$DisplayName" -PublisherUniqueName $PublisherUniqueName -VersionNumber "$VersionNumber" -Description "$Description" -Timeout $crmConnectionTimeout</code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "XrmSolution")]
    public class AddXrmSolution : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The unique name of the solution to be created.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string UniqueName { get; set; }

        /// <summary>
        /// <para type="description">The display name of the solution to be created.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string DisplayName { get; set; }

        /// <summary>
        /// <para type="description">The unique name of the publisher</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string PublisherUniqueName { get; set; }

        /// <summary>
        /// <para type="description">The version number of the solution</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string VersionNumber { get; set; }

        /// <summary>
        /// <para type="description">The description of the solution</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Description { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            base.WriteVerbose("Executing AddXrmSolution");

            base.WriteVerbose(string.Format("Searching for Publisher: {0}", PublisherUniqueName));

            QueryByAttribute queryPublishers = new QueryByAttribute("publisher");
            queryPublishers.Attributes.Add("uniquename");
            queryPublishers.ColumnSet = new ColumnSet(true);
            queryPublishers.Values.Add(PublisherUniqueName);

            EntityCollection publishers = OrganizationService.RetrieveMultiple(queryPublishers);

            base.WriteVerbose(string.Format("# of Publishers found: {0}", publishers.Entities.Count));

            if (publishers.Entities.Count != 1)
            {
                throw new Exception(string.Format("Unique Publisher with name '{0}' was not found", PublisherUniqueName));
            }

            Entity publisher = publishers[0];

            base.WriteVerbose(string.Format("Publisher Located Display Name: {0}, Id: {1}", publisher.Attributes["friendlyname"], publisher.Id));

            base.WriteVerbose("Creating Solution");

            Solution newSolution = new Solution();

            newSolution.UniqueName = UniqueName;
            newSolution.FriendlyName = DisplayName;
            newSolution.Version = VersionNumber;
            newSolution.Description = Description;
            newSolution.PublisherId = publisher.ToEntityReference();

            Guid solutionId = OrganizationService.Create(newSolution);

            base.WriteVerbose(string.Format("Solution Created with Id: {0}", solutionId));

            base.WriteObject(solutionId);

            base.WriteVerbose("Completed AddXrmSolution");
        }

        #endregion
    }
}