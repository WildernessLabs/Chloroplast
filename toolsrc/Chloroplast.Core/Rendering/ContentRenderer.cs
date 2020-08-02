using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Core.Rendering
{
    public class RenderedContent
    {
        public ContentNode Node {get;set;}
        public string Body {get;set;}
        public IDictionary<string,string> Metadata = new Dictionary<string,string>();
        public string GetMeta(string key)
        {
            string value = string.Empty;
            Metadata.TryGetValue (key, out value);
            return value;
        }
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
            var parsed = new RenderedContent 
            {
                Node = node,
                Body = await node.Source.ReadContentAsync()
            };

            // parse front-matter
            YamlRenderer yamlrenderer = new YamlRenderer ();
            (var yaml, string markdown) = yamlrenderer.ParseDoc (parsed.Body);
            parsed.Metadata = yaml;
            parsed.Body = markdown;

            // convert markdown to html
            MarkdownRenderer mdRenderer = new MarkdownRenderer ();
            parsed.Body = mdRenderer.Render (parsed.Body);

            return parsed;
        }

        public static async Task<RenderedContent> ToRazorAsync(RenderedContent content)
        {
            content.Body = await razorRenderer.RenderAsync (content);
            return content;
        }
    }
}
