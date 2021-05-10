using System;
using System.IO;
using Xunit;
using EcmaXml = Choroplast.Core.Loaders.EcmaXml;

namespace Chloroplast.Test
{
    public class EcmaXmlTests
    {
        [Fact]
        public void LoadNamespace()
        {
            EcmaXml.Namespace ns;

            using (var s = GenerateStreamFromString (XmlForNS))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer (typeof (EcmaXml.Namespace));

                ns = (EcmaXml.Namespace)serializer.Deserialize (s);
            }

            Assert.Equal ("Meadow.Foundation.Audio", ns.Name);
            Assert.Equal ("To be added.", ns.Summary);
        }

        public static Stream GenerateStreamFromString (string s)
        {
            var stream = new MemoryStream ();
            var writer = new StreamWriter (stream);
            writer.Write (s);
            writer.Flush ();
            stream.Position = 0;
            return stream;
        }

        private static string XmlForNS = @"<Namespace Name=""Meadow.Foundation.Audio"">
  <Docs>
    <summary>To be added.</summary>
    <remarks>To be added.</remarks>
  </Docs>
</Namespace>
";
    }
}
