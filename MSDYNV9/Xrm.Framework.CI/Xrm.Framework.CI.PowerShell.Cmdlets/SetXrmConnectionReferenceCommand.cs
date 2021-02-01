using System;
using System.Linq;
using System.Management.Automation;
using Xrm.Framework.CI.Common;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Sets a CDS Connection Reference.</para>
    /// <para type="description">This cmdlet links a connection reference to an existing connection
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Set-XrmConnectionReference -Name "new_Name" -ConnectionId "id""</code>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmConnectionReference")]
    public class SetXrmConnectionReferenceCommand : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The LogicalName  of the connection reference</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The Id of the connection</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ConnectionId { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            ConnectionReferenceManager manager = new ConnectionReferenceManager(Logger, OrganizationService);

            manager.SetConnectionReference(Name, ConnectionId);
        }

        #endregion
    }
}
