using System;
using System.Collections.Generic;
using System.IO;
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
            string outputFolder,
            string folder,
            SolutionPackager_PackageType packageType,
            bool includeVersionInName,
            string mappingFile,
            bool treatWarningsAsErrors,
            bool incrementReleaseVersion,
            string version,
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

            Logger.LogInformation("Packing Solution Name: {0} - Version {1}", info.UniqueName, info.Version);

            string newVersion;

            if (incrementReleaseVersion)
            {
                Logger.LogVerbose("Incrementing release version");

                int release = Int32.Parse(info.Version.Substring(info.Version.LastIndexOf(".") + 1));
                newVersion = $"{info.Version.Substring(0, info.Version.LastIndexOf(".") + 1)}{release + 1}";

                solutionXml.UpdateSolutionVersion(folder, newVersion);
            }
            else if (!string.IsNullOrEmpty(version))
            {
                Logger.LogInformation("Updating solution version to {0}", version);

                solutionXml.UpdateSolutionVersion(folder, version);

                newVersion = version;
            }
            else
            {
                newVersion = info.Version;
            }

            SolutionNameGenerator generator = new SolutionNameGenerator();

            string zipFile;
            bool managed = packageType == SolutionPackager_PackageType.Managed;

            if (includeVersionInName)
            {
                zipFile = generator.GetZipName(
                    info.UniqueName,
                    newVersion,
                    managed);
            }
            else
            {
                zipFile = generator.GetZipName(
                    info.UniqueName,
                    string.Empty,
                    managed);
            }
            string zipFilePath = $"{outputFolder}\\{zipFile}";

            Logger.LogVerbose("zipFile: {0}", zipFilePath);

            string log = string.Empty;

            if (!string.IsNullOrEmpty(logsDirectory))
            {
                log = $"{logsDirectory}\\PackagerLog_{zipFile.Replace(".zip", "")}_{DateTime.Now.ToString("yyyy_MM_dd__HH_mm")}.txt";
            }

            Logger.LogVerbose("log: {0}", log);

            SolutionPackager packager = new SolutionPackager(
                Logger,
                solutionPackager,
                zipFilePath,
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
            if (!File.Exists(solutionPackager))
            {
                throw new Exception(string.Format("SolutionPackager.exe file couldn't be found at {0}", solutionPackager));
            }

            if (!Directory.Exists(outputFolder))
            {
                throw new Exception(string.Format("outputFolder couldn't be found at {0}", outputFolder));
            }

            if (!File.Exists(configFilePath))
            {
                throw new Exception(string.Format("Config file couldn't be found at {0}", configFilePath));
            }

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
                    outputFolder,
                    folder,
                    option.PackageType,
                    option.IncludeVersionInName,
                    mapping,
                    option.TreatWarningsAsErrors,
                    option.IncrementReleaseVersion,
                    option.Version,
                    logsDirectory
                    );

                results.Add(result);

                if (!result)
                {
                    break;
                }
            }

            Logger.LogInformation("{0} solutions processed out of {1}", results.Count, config.Solutions.Count);

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
        public bool TreatWarningsAsErrors { get; set; }
        public bool IncrementReleaseVersion { get; set; }
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
