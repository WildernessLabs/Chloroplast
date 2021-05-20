using System;
using System.IO;
using Chloroplast.Core.Loaders;
using Choroplast.Core.Loaders.EcmaXml;
using Xunit;
using EcmaXml = Choroplast.Core.Loaders.EcmaXml;

namespace Chloroplast.Test
{
    public class EcmaXmlTests
    {
        [Fact]
        public void TestLoadNamespace()
        {
            var ns = EcmaXmlLoader.LoadNamespace (XmlForNS);

            Assert.Equal ("Meadow.Foundation.Audio", ns.Name);
            Assert.Equal ("To be added.", ns.Summary);
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
