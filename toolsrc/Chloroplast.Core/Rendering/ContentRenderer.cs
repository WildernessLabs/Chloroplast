using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chloroplast.Core.Rendering
{
    public class RenderedContent
    {
        public ContentNode Node {get;set;}
        public string Body {get;set;}
        public IDictionary<string,string> Metadata = new Dictionary<string,string>();
    }
    public static class ContentRenderer
    {
        public static async Task<RenderedContent> FromMarkdownAsync(ContentNode node)
        {
            var parsed = new RenderedContent 
            {
                Node = node,
                Body = await node.Source.ReadContentAsync()
            };

            // parse front-matter

            // convert markdown to html

            // TODO: better object model for content returned, to include metadata and content
            return parsed;
        }

        public static async Task<RenderedContent> ToRazorAsync(RenderedContent content)
        {
            // TODO: placeholder for razor rendering
            content.Body = $"<html>{content.Body}</html>";
            
            return await Task.Factory.StartNew(() => content);
        }
    }
}
