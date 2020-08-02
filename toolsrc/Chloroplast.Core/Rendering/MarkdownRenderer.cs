using System;
using System.IO;
using System.Threading.Tasks;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Microsoft.Extensions.Configuration;
using Chloroplast.Core.Extensions;

namespace Chloroplast.Core.Rendering
{
    public class MarkdownRenderer
    {
        public string Render (string markdown)
        {
            var builder = new MarkdownPipelineBuilder ()
                .UseEmphasisExtras ()
                .UseGridTables ()
                .UsePipeTables ()
                .UseGenericAttributes ();

            var pipeline = builder.Build ();

            var writer = new StringWriter ();
            var renderer = new HtmlRenderer (writer);
            pipeline.Setup (renderer);

            var document = MarkdownParser.Parse (markdown, pipeline);
            renderer.Render (document);
            writer.Flush ();

            return writer.ToString ();
        }
    }
}
