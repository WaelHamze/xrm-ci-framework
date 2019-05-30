using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.Common
{
    public class SolutionNameGenerator
    {
        public string GetZipName(
            string name,
            string version,
            bool managed)
        {
            var solutionFile = new StringBuilder();

            solutionFile.Append(name);

            if (!string.IsNullOrEmpty(version))
            {
                solutionFile.Append("_");
                solutionFile.Append(version.Replace(".", "_"));
            }

            if (managed)
            {
                solutionFile.Append("_managed");
            }

            solutionFile.Append(".zip");

            return solutionFile.ToString();
        }
    }
}
