using System;
using System.IO;
using System.Xml.Serialization;
using EcmaXml = Chloroplast.Core.Loaders.EcmaXml;

namespace Chloroplast.Core.Loaders
{
    public static class EcmaXmlLoader
    {
        public static EcmaXml.Namespace LoadNamespace (string s) => LoadNamespace (GenerateStreamFromString (s));
        public static EcmaXml.Namespace LoadNamespace (Stream s) => Deserialize<EcmaXml.Namespace> (s);

        public static EcmaXml.Type LoadType (string s) => LoadType (GenerateStreamFromString (s));
        public static EcmaXml.Type LoadType (Stream s) => Deserialize<EcmaXml.Type> (s);

        public static EcmaXml.XType LoadXType (string s) => LoadXType (GenerateStreamFromString (s));
        public static EcmaXml.XType LoadXType (Stream s) => Deserialize<EcmaXml.XType> (s);

        public static Stream GenerateStreamFromString (string s)
        {
            var stream = new MemoryStream ();
            var writer = new StreamWriter (stream);
            writer.Write (s);
            writer.Flush ();
            stream.Position = 0;
            return stream;
        }

        private static T Deserialize<T> (Stream s)
        {
            T ns;
            using (s)
            {
                var serializer = new XmlSerializer (typeof (T));

                ns = (T)serializer.Deserialize (s);
            }

            return ns;
        }
    }
}
