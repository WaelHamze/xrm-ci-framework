using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public enum SolutionPackager_Action
    {
        Extract = 0,
        Pack = 1
    }

    public enum SolutionPackager_PackageType
    {
        Unmanaged = 0,
        Managed = 1,
        Both = 2
    }

    public enum SolutionPackager_ErrorLevel
    {
        Off = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Verbose = 4
    }

    public class SolutionPackager
    {
        #region Properties

        public ILogger Logger { get; set; }

        public string Packager { get; set; }

        public string Folder { get; set; }
        public string ZipFile { get; set; }
        public string Log { get; set; }
        protected bool warnings { get; set; }
        protected bool errors { get; set; }

        #endregion

        #region Constructors

        public SolutionPackager(
            ILogger logger,
            string packager,
            string zipFile,
            string folder,
            string log
            )
        {
            Logger = logger;
            Packager = packager;
            ZipFile = zipFile;
            Folder = folder;
            Log = log;
        }

        #endregion

        #region Methods

        public bool Pack(
            SolutionPackager_PackageType packageType,
            string mappingFile,
            string sourceLoc,
            bool localize,
            bool treatWarningsAsErrors)
        {
            return Execute(
                SolutionPackager_Action.Pack,
                packageType,
                null,
                null,
                false,
                SolutionPackager_ErrorLevel.Verbose,
                mappingFile,
                true,
                Log,
                sourceLoc,
                localize,
                treatWarningsAsErrors);
        }

        public bool Extract(
            SolutionPackager_PackageType packageType,
            string mappingFile,
            bool? allowWrite,
            bool? allowDelete,
            bool clobber,
            string sourceLoc,
            bool localize,
            bool treatWarningsAsErrors)
        {
            return Execute(
                SolutionPackager_Action.Extract,
                packageType,
                allowWrite,
                allowDelete,
                clobber,
                SolutionPackager_ErrorLevel.Verbose,
                mappingFile,
                true,
                Log,
                sourceLoc,
                localize,
                treatWarningsAsErrors
                );
        }

        protected bool Execute(
            SolutionPackager_Action action,
            SolutionPackager_PackageType packageType,
            bool? allowWrite,
            bool? allowDelete,
            bool clobber,
            SolutionPackager_ErrorLevel? errorlevel,
            string map,
            bool nologo,
            string log,
            string sourceLoc,
            bool localize,
            bool treatWarningsAsErrors
            )
        {
            StringBuilder arguments = new StringBuilder();

            Logger.LogVerbose("Generating Solution Packager Arguments");

            arguments.Append($" /action:{action.ToString()}");
            arguments.Append($" /zipFile:\"{ZipFile}\"");
            arguments.Append($" /folder:\"{Folder}\"");
            arguments.Append($" /packageType:{packageType.ToString()}");
            if (allowWrite.HasValue)
            {
                if (allowWrite.Value)
                {
                    arguments.Append($" /allowWrite:Yes");
                }
                else
                {
                    arguments.Append($" /allowWrite:No");
                }
            }
            if (allowDelete.HasValue)
            {
                if (allowDelete.Value)
                {
                    arguments.Append($" /allowDelete:Yes");
                }
                else
                {
                    arguments.Append($" /allowDelete:No");
                }
            }
            if (clobber)
            {
                arguments.Append($" /clobber");
            }
            if (errorlevel.HasValue)
            {
                arguments.Append($" /errorlevel:{errorlevel.Value.ToString()}");
            }
            if (!string.IsNullOrEmpty(map))
            {
                arguments.Append($" /map:\"{map}\"");
            }
            if (nologo)
            {
                arguments.Append($" /nologo");
            }
            if (!string.IsNullOrEmpty(log))
            {
                arguments.Append($" /log:\"{log}\"");
            }
            if (!string.IsNullOrEmpty(sourceLoc))
            {
                arguments.Append($" /sourceLoc:\"{sourceLoc}\"");
            }
            if (localize)
            {
                arguments.Append($" /localize");
            }

            Logger.LogInformation("Solution Packager Version: {0}", FileUtilities.GetFileVersion(Packager));
            Logger.LogInformation("Solution Packager Arguments: {0}", arguments.ToString());

            ProcessStartInfo packagerInfo = new ProcessStartInfo()
            {
                FileName = Packager,
                Arguments = arguments.ToString(),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            int exitCode = -1;

            using (Process packagerProcess = new Process())
            {
                packagerProcess.StartInfo = packagerInfo;
                packagerProcess.OutputDataReceived += PackagerProcess_OutputDataReceived;
                packagerProcess.ErrorDataReceived += PackagerProcess_ErrorDataReceived;

                Logger.LogVerbose("Invoking SolutionPackager: {0}", Packager);

                bool start = packagerProcess.Start();
                packagerProcess.BeginOutputReadLine();
                packagerProcess.BeginErrorReadLine();

                packagerProcess.WaitForExit(1000*60*15); //15 minutes

                //Add sleep to allow time for output streams to flush
                Thread.Sleep(5 * 1000);

                exitCode = packagerProcess.ExitCode;

                Logger.LogInformation("SolutionPackager exit code: {0}", exitCode);

                packagerProcess.Close();
            }

            if (warnings)
            {
                Logger.LogWarning("Solution Packager encountered warnings. Check logs for more information.");
            }
            if (errors)
            {
                Logger.LogError("Solution Packager encountered errors. Check logs for more information.");
            }

            if (exitCode != 0)
            {
                return false;
            }
            else
            {
                if (treatWarningsAsErrors)
                {
                    if (warnings)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        private void PackagerProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e != null)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errors = true;
                }
                if (e.Data != null)
                {
                    Logger.LogInformation(e.Data);
                }
            }
        }

        private void PackagerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e != null)
            {
                if (!string.IsNullOrEmpty(e.Data) && e.Data.Contains("warnings encountered"))
                {
                    warnings = true;
                }
                if (e.Data != null)
                {
                    Logger.LogInformation(e.Data);
                }
            }
        }

        #endregion
    }
}
