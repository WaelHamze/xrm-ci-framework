using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Entities;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class ConnectionReferenceManager : XrmBase
    {
        #region Variables


        #endregion

        #region Constructors

        public ConnectionReferenceManager(ILogger logger,
            IOrganizationService organizationService)
            : base(logger, organizationService)
        {

        }

        #endregion

        #region Methods

        public void SetConnectionReference(string name, string connectionId)
        {
            Logger.LogVerbose("Setting ConnectionReference with LogicalName {0} to Connection with Id {1}", name, connectionId);

            connectionreference current = GetConnectionReference(name);

            if (current != null)
            {
                if (current.ConnectionId != connectionId)
                {
                    current.ConnectionId = connectionId;

                    connectionreference update = new connectionreference();
                    update.Id = current.Id;
                    update.ConnectionId = connectionId;
                    OrganizationService.Update(update);

                    Logger.LogInformation("Updated ConnectionReference LogicalName ={0} to Connection with Id {1}", name, connectionId);
                }
                else
                {
                    Logger.LogInformation("Skipped update of ConnectionReference LogicalName ={0} to Connection with Id {1} as this is already linked", name, connectionId);
                }
            }
            else
            {
                throw new Exception($"Connection Reference with LogicalName {name} does not exist.");
            }
        }

        private connectionreference GetConnectionReference(string name)
        {
            Logger.LogVerbose("Retrieving ConnectionReference with LogicalName {0}", name);

            using (var context = new CIContext(OrganizationService))
            {
                var query = from conRef in context.connectionreferenceSet
                            where conRef.ConnectionReferenceLogicalName == name
                            select conRef;

                var references = query.ToList<connectionreference>();

                if (references.Count == 0)
                {
                    Logger.LogVerbose("No ConnectionReference records found");
                    return null;
                }
                else if (references.Count == 1)
                {
                    Logger.LogVerbose("ConnectionReference record found with Id = {0}", references[0].Id);
                    return references[0];
                }
                else
                {
                    throw new Exception($"Mutiple ConnectionReferences with LogicalName = {name}");
                }
            }
        }

        #endregion
    }

    //public Component
}
