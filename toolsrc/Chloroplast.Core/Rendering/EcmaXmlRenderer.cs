using System;
using System.Threading.Tasks;
using Chloroplast.Core.Loaders;
using EcmaXml = Chloroplast.Core.Loaders.EcmaXml;

namespace Chloroplast.Core.Rendering
{
    public class EcmaXmlRenderer
    {
        public EcmaXmlRenderer ()
        {
        }

        public static async Task<RenderedContent> Render (ContentNode item, string body, Microsoft.Extensions.Configuration.IConfigurationRoot config)
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
                
                var result = await ContentRenderer.ToRazorAsync (nscontent);
                //return md.Render (ns.Summary);

                var content = new RenderedContent
                {
                    Body = result,
                    Node = item
                };

                return content;
            }

            var def = new RenderedContent
            {
                Body = System.Web.HttpUtility.HtmlEncode(body),
                Node = item
            };

            return def;
        }
    }
}
