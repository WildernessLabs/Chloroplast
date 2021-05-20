using System;
using Chloroplast.Core.Loaders;
using EcmaXml = Choroplast.Core.Loaders.EcmaXml;

namespace Chloroplast.Core.Rendering
{
    public class EcmaXmlRenderer
    {
        public EcmaXmlRenderer ()
        {
        }

        public static RenderedContent Render (ContentNode item, string body, Microsoft.Extensions.Configuration.IConfigurationRoot config)
        {
            if (body.StartsWith("<Namespace"))
            {
                var ns = EcmaXmlLoader.LoadNamespace (body);

                MarkdownRenderer md = new MarkdownRenderer ();

                EcmaXmlContent<EcmaXml.Namespace> nscontent = new EcmaXmlContent<EcmaXml.Namespace>
                {
                    Node = item,
                    Element = ns,
                    Metadata = config
                };

                ContentRenderer.ToRazorAsync (nscontent);
                return md.Render (ns.Summary);
            }
        }
    }
}
