using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Chloroplast.Core.Extensions;

namespace Chloroplast.Core.Rendering
{
    public class RenderedContent
    {
        public ContentNode Node { get; set; }
        public string Body { get; set; }
        public IDictionary<string, string> Metadata = new Dictionary<string, string> ();

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
            string value;
            Metadata.TryGetValue (key, out value);
            return value;
        }

        public bool HasMeta(string key)
        {
            return Metadata.ContainsKey (key);
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
            parsed.Node.Title = yaml.Try("title") ?? yaml.Try("Title") ?? parsed.Node.Slug;

            // convert markdown to html
            MarkdownRenderer mdRenderer = new MarkdownRenderer ();
            parsed.Body = mdRenderer.Render (parsed.Body);

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
