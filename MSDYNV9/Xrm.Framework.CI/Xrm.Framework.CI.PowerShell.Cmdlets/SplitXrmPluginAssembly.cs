using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Split a plugin assembly</para>
    /// <para type="description">This cmdlet splits the plugin assembly into multiple pluginn assemblies based on class files filteredd by regex.
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Split-XrmPluginAssembly -regexType "" -regex "" -projectFilePath ""</code>
    ///   <para>This cmdlet splits the plugin assembly into multiple pluginn assemblies based on class files filteredd by regex.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Split, "XrmPluginAssembly")]
    public class SplitXrmPluginAssembly : Cmdlet
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The absolute path to the solution file to be imported</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string regexType { get; set; }

        [Parameter(Mandatory = true)]
        public string regex { get; set; }
        
        [Parameter(Mandatory = true)]
        public string projectFilePath { get; set; }

        [Parameter(Mandatory = false)]
        public string solutionFilePath { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var temp = new FileInfo(projectFilePath);
            var targetDirectory = temp.DirectoryName;
            var sourceDirectory = temp.DirectoryName;
            var projectFileName = temp.Name;
            var pluginAssemblyName = GetProjectPropertyFromProjectFile(projectFilePath, "AssemblyName");
            var files = GetFilteredFiles(sourceDirectory);

            foreach (string file in files)
            {
                base.WriteDebug("New project creation started - " + file);
                var fileDetails = new FileInfo(file);
                var newdir = targetDirectory + pluginAssemblyName + "." + fileDetails.Name.Replace(".cs", "");

                Copy(sourceDirectory, newdir);
                base.WriteDebug("Copy complete to new directory - " + newdir);
                // Replace Xrm.CI.Framework.Sample.Plugins by appending file name
                string str = File.ReadAllText(newdir + @"\" + projectFileName);
                str = str.Replace("<AssemblyName>" + pluginAssemblyName + "</AssemblyName>"
                    , "<AssemblyName>" + pluginAssemblyName + "." + fileDetails.Name.Replace(".cs", "") + "</AssemblyName>");
                str = str.Replace("'$(Configuration)|$(Platform)'", "'$(Configuration)'");
                str = str.Replace("'Debug|AnyCPU'", "'Debug'");
                str = str.Replace("'Release|AnyCPU'", "'Release'");
                var files1 = GetFilteredFiles(newdir);
                // Remove other cs files
                foreach (string file1 in files1)
                {
                    var fileDetails1 = new FileInfo(file1);
                    if (!fileDetails.Name.Equals(fileDetails1.Name))
                    {
                        str = str.Replace("<Compile Include=\"" + fileDetails1.Name + "\" />", "");

                         File.Delete(file1);
                        base.WriteDebug("Delete complete - " + file1);
                    }
                }

                File.WriteAllText(newdir + @"\" + projectFileName, str);
                base.WriteDebug("New project creation completed - " + newdir + @"\" + projectFileName);
            }

            if (!string.IsNullOrEmpty(solutionFilePath))
            {
                AddProjectToSolution(solutionFilePath, projectFilePath, projectFileName);
            }
        }

        private IEnumerable<string> GetFilteredFiles(string sourceDirectory)
        {            
            var files = Directory.GetFiles(sourceDirectory)
                .Where(file => (file.EndsWith(".cs") || file.EndsWith(".vb")));
            if (regexType.Equals("filename", StringComparison.InvariantCultureIgnoreCase))
            {
                var reg = new Regex(regex, RegexOptions.Singleline);
                files = files.Where(file => reg.IsMatch(Path.GetFileName(file)));
            }
            else if (regexType.Equals("filecontaint", StringComparison.InvariantCultureIgnoreCase))
            {                
                var reg = new Regex(regex, RegexOptions.Singleline);
                files = files.Where(file => reg.IsMatch(File.ReadAllText(file)));
            }
            else
            {
                throw new Exception("Not implemented");
            }

            return files;
        }

        private void AddProjectToSolution(string solutionFilePath, string projectFilePath, string projectFileName)
        {
            var projectGuid = GetProjectPropertyFromProjectFile(projectFilePath, "ProjectGuid");
            string[] textLines = File.ReadAllLines(solutionFilePath);
            string line = textLines.Where(l => l.StartsWith("Project(\"") && l.Contains(projectGuid)).FirstOrDefault();
            if (string.IsNullOrEmpty(line))
            { throw new Exception(string.Format("Project {0} not found in solution {1}.", projectFilePath, solutionFilePath)); }

            var solutionDirectory = Path.GetDirectoryName(solutionFilePath);
            var projects = Directory.EnumerateFiles(solutionDirectory, projectFileName, SearchOption.AllDirectories);
            var writer = new StreamWriter(solutionFilePath, true, Encoding.UTF8);
            var relativePath = projectFilePath.Replace(solutionDirectory + "\\", "");
            var solutionGuid = line.Substring(line.IndexOf("(\"") + 2, line.IndexOf("\")") - line.IndexOf("(\"") - 2);
            int counter = 1;
            foreach (var project in projects)
            {
                if (project.Equals(projectFilePath, StringComparison.InvariantCultureIgnoreCase)) continue;
                string str = File.ReadAllText(project);
                var fileName = Path.GetFileNameWithoutExtension(project) + counter++ + Path.GetExtension(project);
                var newProjectGuid = Guid.NewGuid().ToString().ToUpper();
                str = str.Replace(projectGuid, "{" + newProjectGuid + "}");
                File.WriteAllText(project, str);
                File.Move(project, project.Replace(Path.GetFileName(project), fileName));

                var projectLine = string.Concat("Project(\""
                                                , solutionGuid, "\") = \""
                                                , fileName, "\", \""
                                                , project.Replace(solutionDirectory + "\\", "").Replace(Path.GetFileName(project), fileName)
                                                , "\", \"{"
                                                , newProjectGuid, "}\"");
                writer.WriteLine(projectLine);
                writer.WriteLine("EndProject");
            }

            writer.Flush();
            writer.Close();
            writer.Dispose();            
        }       

        private string GetProjectPropertyFromProjectFile(string projectFilepath, string projectProperty)
        {
            XNamespace xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            XDocument projDefinition = XDocument.Load(projectFilepath);

            IEnumerable<XNode> resultsEnumerable = projDefinition
                .Element(xmlns + "Project")
                .Elements(xmlns + "PropertyGroup")
                .Elements(xmlns + projectProperty).Nodes<XContainer>();

            IList<XNode> results = new List<XNode>(resultsEnumerable);
            if (results.Count == 0)
            {
                throw new Exception(String.Format("The project file isn't correctly structured: [{0}]", projectFilepath));
            }

            string propertyName = results[0].ToString();

            return propertyName;
        }

        private void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        #endregion
    }
}