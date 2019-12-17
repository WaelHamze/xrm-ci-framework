using DocumentFormat.OpenXml.Packaging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.Common
{
    /// <summary>
    /// code from https://github.com/MscrmTools/MscrmTools.DocumentTemplatesMover/blob/master/MsCrmTools.DocumentTemplatesMover/TemplatesManager.cs
    /// </summary>
    public static class TemplateManager
    {
        public static string ReRouteEtcViaOpenXML(string base64content, string etc, int? oldEtc, int? newEtc)
        {
            using (var contentStream = new MemoryStream())
            {
                var bytes = Convert.FromBase64String(base64content);
                contentStream.Write(bytes, 0, bytes.Length);
                contentStream.Position = 0;

                string toFind = $"{etc}/{oldEtc}";
                string replaceWith = $"{etc}/{newEtc}";

                using (var doc = WordprocessingDocument.Open(contentStream, true, new OpenSettings { AutoSave = true }))
                {
                    // crm keeps the etc in multiple places; parts here are the actual merge fields
                    doc.MainDocumentPart.Document.InnerXml = doc.MainDocumentPart.Document.InnerXml.Replace(toFind, replaceWith);

                    // next is the actual namespace declaration
                    doc.MainDocumentPart.CustomXmlParts.ToList().ForEach(a =>
                    {
                        using (StreamReader reader = new StreamReader(a.GetStream()))
                        {
                            var xml = XDocument.Load(reader);

                        // crappy way to replace the xml, couldn't be bothered figuring out xml root attribute replacement...
                        var crappy = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" + xml.ToString();

                            if (crappy.IndexOf(toFind) > -1) // only replace what is needed
                        {
                                crappy = crappy.Replace(toFind, replaceWith);

                                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(crappy)))
                                {
                                    a.FeedData(stream);
                                }
                            }
                        }
                    });
                }

                return Convert.ToBase64String(contentStream.ToArray());
            }
        }
    }
}
