using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Chloroplast.Core.Extensions;
using Chloroplast.Core.Content;

namespace Chloroplast.Core.Rendering
{
    public class RenderedContent
    {
        public ContentNode Node { get; set; }
        public string Body { get; set; }
        public IConfigurationRoot Metadata { get; set; }
        public Header[] Headers { get; internal set; }

        public RenderedContent ()
        {
        }

        public RenderedContent (RenderedContent content)
        {
            this.Node = content.Node;
            this.Body = content.Body;
            this.Metadata = content.Metadata;
        }

        public string GetMeta(string key)
        {
            string value = Metadata[key];
            return value ?? string.Empty;
        }

        public bool HasMeta(string key)
        {
            string value = Metadata[key];
            return string.IsNullOrWhiteSpace(value);
        }
    }

    public class FrameRenderedContent : RenderedContent
    {
        public FrameRenderedContent (RenderedContent content, IEnumerable<ContentNode> tree)
            : base (content)
        {
            Tree = tree;
        }

        public IEnumerable<ContentNode> Tree { get; set; }
    }

    public static class ContentRenderer
    {
        private static RazorRenderer razorRenderer;

        public static async Task InitializeAsync(IConfigurationRoot config)
        {
            razorRenderer = new RazorRenderer ();
            await razorRenderer.InitializeAsync (config);
        }
        public static async Task<RenderedContent> FromMarkdownAsync(ContentNode node)
        {
            var content = node.Source.ReadContentAsync ();
            var parsed = new RenderedContent 
            {
                Node = node,
                Body = content.Result
            };
            
            // parse front-matter
            YamlRenderer yamlrenderer = new YamlRenderer ();
            (var yaml, string markdown) = yamlrenderer.ParseDoc (parsed.Body);
            parsed.Metadata = yaml;

            parsed.Body = markdown;
            parsed.Node.Title = yaml["title"] ?? yaml["Title"] ?? parsed.Node.Slug;

            // convert markdown to html
            MarkdownRenderer mdRenderer = new MarkdownRenderer ();
            parsed.Body = mdRenderer.Render (parsed.Body);

            // parse out headers
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument ();
            doc.LoadHtml (parsed.Body);
            var nodes = doc.DocumentNode.SelectNodes ("//h1|//h2|//h3|//h4//h5//h6");
            List<Header> headers = null;
            if (nodes != null)
            {
                headers = new List<Header> (nodes.Count);
                foreach (var n in nodes.Where(n=>!string.IsNullOrWhiteSpace(n.InnerText)))
                {
                    var header = Header.FromNode (n);
                    headers.Add (header);

                    // insert the anchor before the header element
                    var anchor = doc.CreateElement ("a");
                    anchor.Attributes.Add ("name", header.Slug);
                    n.ParentNode.InsertBefore (anchor, n);
                }
                parsed.Body = doc.DocumentNode.OuterHtml;
            }
            parsed.Headers = headers != null ? headers.ToArray() : new Header[0];

            return parsed;
        }

        public static async Task<RenderedContent> ToRazorAsync (RenderedContent content)
        {
            content.Body = await razorRenderer.RenderContentAsync (content);

            return content;
        }

        public static async Task<RenderedContent> ToRazorAsync (FrameRenderedContent content)
        {
            content.Body = await razorRenderer.RenderContentAsync (content);
            return content;
        }
    }
}
