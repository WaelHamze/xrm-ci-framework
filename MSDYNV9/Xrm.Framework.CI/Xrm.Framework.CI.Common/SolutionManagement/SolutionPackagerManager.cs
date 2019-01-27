using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class SolutionPackagerManager
    {
        protected ILogger Logger
        {
            get;
            set;
        }

        public SolutionPackagerManager(
            ILogger logger)
        {
            Logger = logger;
        }

        public bool PackSolution(
            string solutionPackager,
            string folder,
            SolutionPackager_PackageType packageType,
            bool includeVersionInName,
            string mappingFile,
            bool treatWarningsAsErrors,
            string logsDirectory
            )
        {
            Logger.LogVerbose("Packing Solution from: {0}", folder);

            SolutionXml solutionXml = new SolutionXml(Logger);
            XrmSolutionInfo info = solutionXml.GetXrmSolutionInfoFromFolder(folder);

            if (info == null)
            {
                throw new Exception("Invalid solution file");
            }

            SolutionNameGenerator generator = new SolutionNameGenerator();

            string zipFile;
            bool managed = packageType == SolutionPackager_PackageType.Managed;

            if (includeVersionInName)
            {
                zipFile = generator.GetZipName(
                    info.UniqueName,
                    info.Version,
                    managed);
            }
            else
            {
                zipFile = generator.GetZipName(
                    info.UniqueName,
                    string.Empty,
                    managed);
            }

            string log = $"PackagerLog_{zipFile.Replace(".zip", "")}_{DateTime.Now.ToString("yyyy_MM_dd__HH_mm")}.txt";

            SolutionPackager packager = new SolutionPackager(
                Logger,
                solutionPackager,
                zipFile,
                folder,
                log
                );

            return packager.Pack(
                packageType,
                mappingFile,
                treatWarningsAsErrors);
        }

        public List<bool> PackSolutions(
            string solutionPackager,
            string outputFolder,
            string configFilePath,
            string logsDirectory)
        {
            SolutionPackConfig config =
                Serializers.ParseJson<SolutionPackConfig>(configFilePath);

            List<bool> results = new List<bool>();

            foreach (SolutionPackOptions option in config.Solutions)
            {
                string configDirectory = new FileInfo(configFilePath).DirectoryName;

                string folder = Path.Combine(configDirectory, option.Folder);
                string mapping = Path.Combine(configDirectory, option.MappingFile);

                bool result = PackSolution(
                    solutionPackager,
                    folder,
                    option.PackageType,
                    option.IncludeVersionInName,
                    mapping,
                    option.TreamWarningsAsErrors,
                    logsDirectory
                    );

                results.Add(result);

                if (!result)
                {
                    break;
                }
            }

            return results;
        }
    }

    public class SolutionPackOptions
    {
        #region Properties

        public string Folder { get; set; }
        public string MappingFile { get; set; }
        public SolutionPackager_PackageType PackageType { get; set; }
        public bool IncludeVersionInName { get; set; }
        public bool TreamWarningsAsErrors { get; set; }
        public string Version { get; set; }

        #endregion

        #region Constructors

        public SolutionPackOptions()
        {
            Version = string.Empty;
            IncludeVersionInName = true;
        }

        #endregion
    }

    public class SolutionPackConfig
    {
        #region Properties

        public List<SolutionPackOptions> Solutions { get; set; }

        #endregion

        #region Constructors

        public SolutionPackConfig()
        {
            Solutions = new List<SolutionPackOptions>();
        }

        #endregion
    }
}
