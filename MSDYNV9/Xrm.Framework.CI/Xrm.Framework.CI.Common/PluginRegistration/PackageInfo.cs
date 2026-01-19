using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Xrm.Framework.CI.Common.PluginRegistration
{
    public class PackageInfo
    {
        public string PackageName { get; private set; }

        public string PackageVersion { get; set; }

        public string PackageDirectory { get; private set; }

        public string Content { get; private set; }

        private PackageInfo()
        { }

        public static PackageInfo GetPackageInfo(string packagePath)
        {
            if (packagePath.EndsWith(".nupkg") == false)
                throw new Exception("Invalid package file extension. Only .nupkg files are supported.");

            var zip = ZipFile.OpenRead(packagePath);

            var entry = zip.Entries.FirstOrDefault(x => x.Name.EndsWith("nuspec"));

            if (entry == null)
                throw new Exception("Package does not contain a nuspec file.");

            var doc = XDocument.Load(entry.Open());
            var ns = doc.Root.GetDefaultNamespace();

            var id = doc.Descendants(ns + "id").First().Value;
            var version = doc.Descendants(ns + "version").First().Value;

            var info = new PackageInfo
            {
                PackageName = id,
                PackageVersion = version,
                PackageDirectory = packagePath,
                Content = Convert.ToBase64String(File.ReadAllBytes(packagePath))
            };

            return info;
        }
    }
}