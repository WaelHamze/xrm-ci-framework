using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.Common
{
    public class AssemblyInfo
    {
        public string AssemblyName { get; private set; }

        public string AssemblyDirectory { get; private set; }
        
        public string Version { get; private set; }

        public string Content { get; private set; }

        private AssemblyInfo() { }

        public static AssemblyInfo GetAssemblyInfo(string assemblyPath)
        {
            var info = new AssemblyInfo();
            FileInfo assemblyInfo = new FileInfo(assemblyPath);
            var lastIndex = assemblyInfo.Name.LastIndexOf(".dll");
            info.AssemblyName = lastIndex > 0 ? assemblyInfo.Name.Remove(lastIndex, 4) : assemblyInfo.Name;
            info.AssemblyDirectory = assemblyInfo.DirectoryName;
            info.Version = FileVersionInfo.GetVersionInfo(assemblyPath).FileVersion;
            info.Content = Convert.ToBase64String(File.ReadAllBytes(assemblyPath));

            return info;
        }
    }
}
