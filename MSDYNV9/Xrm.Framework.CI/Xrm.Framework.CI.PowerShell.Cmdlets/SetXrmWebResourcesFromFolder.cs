using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// Updates CRM web resources from folder
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "XrmWebResourcesFromFolder")]
    public class SetXrmWebResourcesFromFolder : XrmCommandBase
    {
        #region Parameters

        /// <summary>
        /// Path to folder with web resource files
        /// </summary>
        [Parameter(Mandatory = true)]
        public String Path { get; set; }

        /// <summary>
        /// Comma separated list of search patterns, e.g. "*.js, *test*.html"
        /// </summary>
        [Parameter(Mandatory = false)]
        public String SearchPattern { get; set; }
        /// <summary>
        /// Publish web resources after update
        /// </summary>
        [Parameter(Mandatory = false)]
        public Boolean Publish { get; set; }

        /// <summary>
        /// Regular expression to match web resourse name with file name
        /// </summary>
        [Parameter(Mandatory = false)]
        public String RegExToMatchUniqueName { get; set; }

        /// <summary>
        /// Include web resource file extension to regular expression matching pattern
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool IncludeFileExtensionForUniqueName { get; set; }

        /// <summary>
        /// Write error if resource was not found in CRM
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool FailIfWebResourceNotFound { get; set; }

        #endregion


        #region Process Record
        protected override void ProcessRecord()
        {
            var missedWebResourcesCount = 0;
            var resourceFiles = new HashSet<string>();
            var patterns = (string.IsNullOrWhiteSpace(SearchPattern)) ? new string[1] { "*" } :
                SearchPattern.Split(',');

            foreach (var pattern in patterns)
            {
                WriteVerbose($"Processing pattern {pattern}...");

                Directory.GetFiles(Path, pattern.Trim(), SearchOption.AllDirectories).ToList()
                    .ForEach((item) => resourceFiles.Add(item));
            }

            if (resourceFiles.Count == 0)
            {
                WriteVerbose($"There are no files in folder '{Path}' matching patterns {SearchPattern}");
                return;
            }
            else
                WriteVerbose($"Found {resourceFiles.Count} resource files.");

            using (var context = new CIContext(OrganizationService))
            {
                WriteVerbose($"Retrieving web resources from CRM...");
                var allWebResources = context.WebResourceSet.ToList();
                WriteVerbose($"Found {allWebResources.Count}");

                var importexportxml = new StringBuilder();
                var webResource = default(WebResource);
                foreach (var resourceFile in resourceFiles)
                {
                    WriteVerbose($"Processing file: {System.IO.Path.GetFileName(resourceFile)}");
                    try
                    {
                        webResource = allWebResources.Single(CompareCondition(resourceFile));
                        WriteVerbose($"Found web resource: {webResource.Name}");
                    }
                    catch (Exception ex)
                    {
                        missedWebResourcesCount++;
                        WriteWarning($"Cannot process {resourceFile}: {ex.Message}");
                        continue;
                    }

                    // update in context
                    var fileContent = Convert.ToBase64String(File.ReadAllBytes(resourceFile));
                    if (webResource.Content?.GetHashCode() != fileContent.GetHashCode())
                    {
                        webResource.Content = fileContent;
                        context.UpdateObject(webResource);
                        // add id to publish xml
                        if (Publish)
                            importexportxml.Append($"<webresource>{webResource.Id}</webresource>");
                    }

                }

                // Update
                WriteVerbose("Saving changes...");
                context.SaveChanges();

                // Publish
                if (Publish)
                {
                    WriteVerbose("Publishing web resources...");
                    PublishXmlRequest req = new PublishXmlRequest()
                    {
                        ParameterXml = $"<importexportxml><webresources>{importexportxml.ToString()}</webresources></importexportxml>"
                    };
                    OrganizationService.Execute(req);
                }

                WriteObject($"{resourceFiles.Count - missedWebResourcesCount} out of {resourceFiles.Count} web resources were processed");

                if (FailIfWebResourceNotFound && missedWebResourcesCount > 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new RuntimeException($"{missedWebResourcesCount} web resources were not found"),
                        "", ErrorCategory.ObjectNotFound, null));
                }
            }
        }

        #endregion

        private Func<WebResource, bool> CompareCondition(string resourceFile)
        {
            resourceFile = (IncludeFileExtensionForUniqueName) ?
                    System.IO.Path.GetFileName(resourceFile) :
                    System.IO.Path.GetFileNameWithoutExtension(resourceFile);

            if (string.IsNullOrWhiteSpace(RegExToMatchUniqueName))
            {
                return x => x.Name == resourceFile;
            }
            else
            {
                return x => Regex.IsMatch(x.Name, RegExToMatchUniqueName.Replace(
                    "{filename}", resourceFile));
            }
        }
    }
}
