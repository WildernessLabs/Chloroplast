using System;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Content;
using Chloroplast.Core.Loaders;
using Microsoft.Extensions.Configuration;
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

            MarkdownRenderer md = new MarkdownRenderer ();

            if (body.StartsWith ("<Type "))
            {
                GroupContentArea groupArea = item.Area as GroupContentArea;

                var t = EcmaXmlLoader.LoadXType (body);

                // TODO: use groupArea.OverridePath to look for the type's override files if any
                // and also all members
                // for uid, use t.FullName
                if (groupArea != null && Directory.Exists(groupArea.OverridePath))
                {
                    var overrideFile = Path.Combine (groupArea.OverridePath,t.Namespace, t.FullName + ".md");
                    if (File.Exists(overrideFile))
                    {
                        t.Docs.Remarks = File.ReadAllText (overrideFile);
                    }

                    foreach(var m in t.Members)
                    {
                        // TODO: look for override file for this member
                        overrideFile = Path.Combine (groupArea.OverridePath, t.Namespace, t.FullName + "." + m.Name + ".md");
                        if (File.Exists (overrideFile))
                        {
                            m.Docs.Remarks = File.ReadAllText (overrideFile);
                        }
                    }
                }

                var nscontent = ToEcmaContent (item, config, t);

                var result = await ContentRenderer.ToRazorAsync (nscontent);

                var content = new RenderedContent
                {
                    Body = result,
                    Node = item
                };

                return content;
            }
            else if (body.StartsWith("<Namespace"))
            {
                // TODO: Load type list for this namespace
                var ns = EcmaXmlLoader.LoadNamespace (body);

                var nscontent = ToEcmaContent (item, config, ns);

                var result = await ContentRenderer.ToRazorAsync (nscontent);

                RenderedContent content = ToRenderedContent (item, result);

                return content;
            }

            var def = new RenderedContent
            {
                Body = System.Web.HttpUtility.HtmlEncode(body),
                Node = item
            };

            return def;
        }

        private static RenderedContent ToRenderedContent (ContentNode item, string result)
        {
            return new RenderedContent
            {
                Body = result,
                Node = item
            };
        }

        private static EcmaXmlContent<T> ToEcmaContent<T> (ContentNode item, IConfigurationRoot config, T t)
        {
            return new EcmaXmlContent<T>
            {
                Node = item,
                Element = t,
                Metadata = config
            };
        }
    }
}
