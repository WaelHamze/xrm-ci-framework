using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.Common.Logging;

namespace Xrm.Framework.CI.Common
{
    public class FileUtilities
    {
        // Just for safety
        public static readonly int DirectoryRecursionLimit = 8;

        public static string GetFileVersion(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);

            return versionInfo.FileVersion;
        }

        public static string GetFileSize(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(fileName).Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);

            return result;
        }

        // Warning - Recursive function (required to trawl directory structure)
        public static DirectoryInfo DirectoryCopy(string sourceDirName, string destDirName, ILogger logger = null, bool copySubDirs = false, HashSet<string> subDirWhiteList = null, int recursionCount = 0)
        {
            if (recursionCount > DirectoryRecursionLimit)
            {
                logger?.LogWarning($"Directory copy recursion limit of {DirectoryRecursionLimit} reached, preventing further recursion.");
                return null;
            }

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            DirectoryInfo destDirInfo = null;
            if (!Directory.Exists(destDirName))
            {
                destDirInfo = Directory.CreateDirectory(destDirName);
                logger?.LogVerbose($"Created Temp Dir : {destDirName}");
            }
            else
            {
                destDirInfo = new DirectoryInfo(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                var subDirSet = dirs.AsQueryable();
                if (subDirWhiteList != null)
                {
                    subDirSet = subDirSet.Where(d => subDirWhiteList.Contains(d.Name));
                }
                recursionCount++;
                foreach (DirectoryInfo subdir in subDirSet)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, logger, copySubDirs, subDirWhiteList, recursionCount);
                }
            }

            return destDirInfo;
        }
    }
}
