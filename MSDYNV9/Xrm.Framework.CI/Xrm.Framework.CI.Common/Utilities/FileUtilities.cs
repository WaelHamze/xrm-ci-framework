using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.Common
{
    public class FileUtilities
    {
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
    }
}
