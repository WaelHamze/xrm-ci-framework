using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;

namespace Xrm.Framework.CI.Common.Workflows
{
    public static class XamlValuesHelper
    {
        private static string ExtractField(string s, int n)
        {
            var tmp = s.Substring(s.IndexOf("{") + 1, s.LastIndexOf("}") - s.IndexOf("{") - 1).Trim();
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tmp)))
            {
                using (var parser = new TextFieldParser(stream))
                {
                    parser.SetDelimiters(",");
                    var arr = parser.ReadFields();
                    return arr.Length >= n ? arr[n] : null;
                }
            }
        }

        public static Guid ExtractGuidValueFromGuidParameter(string s) => Guid.Parse(ExtractField(s, 1));

        public static string ExtractNameFromLookup(string s) => ExtractField(s, 2);

        public static string ExtractEntityLogicalNameFromLookup(string s) => ExtractField(s, 1);

        public static string ExtractGuidParameterNameFromLookup(string s) => ExtractField(s, 3);
    }
}