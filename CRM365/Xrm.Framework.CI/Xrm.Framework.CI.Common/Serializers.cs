using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Xrm.Framework.CI.Common
{
    public static class Serializers
    {
        public static void SaveJson<T>(string filename, T obj) where T: class
        {
            JsonConvert.DefaultSettings = (() => new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() },
                Formatting = Newtonsoft.Json.Formatting.Indented,
            }
            );
            File.WriteAllText(filename, JsonConvert.SerializeObject(obj), Encoding.UTF8);
        }

        public static void SaveXml<T>(string filename, T obj) 
        {
            using (var textwriter = new StreamWriter(filename, false, new UTF8Encoding(true)))
            {
                var settings = new XmlWriterSettings()
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                    //OmitXmlDeclaration = true
                };
                var serializer = new XmlSerializer(typeof(T));
                using (XmlWriter writer = XmlWriter.Create(textwriter, settings))
                {
                    serializer.Serialize(writer, obj);
                }
            }
        }

        public static T ParseXml<T>(string filename) where T : class
        {
            using (var reader = new StreamReader(filename, true))
            {
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(reader) as T;
            }
        }

        public static T ParseJson<T>(string filename) where T : class => JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
    }
}
