using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Chloroplast.Core.Extensions;
using Chloroplast.Core.Content;
using EcmaXml = Chloroplast.Core.Loaders.EcmaXml;

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
            if (Metadata == null) return string.Empty;

            string value = Metadata[key];
            return value ?? string.Empty;
        }

        public bool HasMeta(string key)
        {
            if (Metadata == null) return false;

            string value = Metadata[key];
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Creates a merged metadata configuration by combining parent and child metadata.
        /// Child metadata values override parent values for the same keys.
        /// </summary>
        /// <param name="parentMetadata">Parent configuration (can be null)</param>
        /// <param name="childMetadata">Child configuration (can be null)</param>
        /// <returns>A new merged IConfigurationRoot, or null if both inputs are null</returns>
        public static IConfigurationRoot MergeMetadata(IConfigurationRoot parentMetadata, IConfigurationRoot childMetadata)
        {
            // If both are null, return null
            if (parentMetadata == null && childMetadata == null)
                return null;

            // If only one is provided, return it
            if (parentMetadata == null)
                return childMetadata;
            if (childMetadata == null)
                return parentMetadata;

            // Both exist - merge them with child overriding parent
            var builder = new ConfigurationBuilder();

            // Convert parent metadata to dictionary
            var parentDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in parentMetadata.AsEnumerable())
            {
                if (kvp.Value != null)
                    parentDict[kvp.Key] = kvp.Value;
            }

            // Add parent metadata first
            builder.AddInMemoryCollection(parentDict);

            // Convert child metadata to dictionary
            var childDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in childMetadata.AsEnumerable())
            {
                if (kvp.Value != null)
                    childDict[kvp.Key] = kvp.Value;
            }

            // Add child metadata second (overrides parent)
            builder.AddInMemoryCollection(childDict);

            return builder.Build();
        }
    }

    public class EcmaXmlContent<T> : RenderedContent
    {
        public T Element { get; set; }
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
            
            // Check if content is machine translated
            var machineTranslated = yaml["machineTranslated"] ?? yaml["machine_translated"] ?? yaml["MachineTranslated"];
            if (!string.IsNullOrWhiteSpace(machineTranslated) && 
                (machineTranslated.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                 machineTranslated.Equals("yes", StringComparison.OrdinalIgnoreCase)))
            {
                parsed.Node.IsMachineTranslated = true;
            }

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
            }
            parsed.Headers = headers != null ? headers.ToArray() : new Header[0];

            // Apply BasePath and path normalization to root-relative links
            var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes != null)
            {
                // Check if we should normalize paths based on the content area settings
                bool shouldNormalizePaths = node.Area is GroupContentArea groupArea ? groupArea.NormalizePaths : false;

                foreach (var linkNode in linkNodes)
                {
                    var href = linkNode.GetAttributeValue("href", "");
                    
                    // Only process root-relative links (starting with /)
                    // Skip absolute URLs, fragments, and relative paths
                    if (!string.IsNullOrEmpty(href) && 
                        href.StartsWith("/") && 
                        !href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                        !href.StartsWith("#"))
                    {
                        var updatedHref = href;

                        // Apply path normalization if enabled
                        if (shouldNormalizePaths)
                        {
                            // Normalize the path component (remove basePath temporarily if present, normalize, then reapply)
                            var pathToNormalize = href;
                            if (!string.IsNullOrEmpty(SiteConfig.BasePath) && href.StartsWith(SiteConfig.BasePath))
                            {
                                pathToNormalize = href.Substring(SiteConfig.BasePath.Length);
                            }
                            
                            var normalizedPath = pathToNormalize.NormalizeUrlSegment(toLower: true);
                            if (!normalizedPath.StartsWith("/"))
                            {
                                normalizedPath = "/" + normalizedPath;
                            }

                            updatedHref = normalizedPath;
                        }

                        // Apply locale/base path handling. If already locale-prefixed for this node, just apply base path.
                        if (node.Locale != SiteConfig.DefaultLocale && updatedHref.StartsWith($"/{node.Locale}/"))
                        {
                            updatedHref = SiteConfig.ApplyBasePath(updatedHref); // already localized
                        }
                        else
                        {
                            // For non-default locales, localize links that are not already localized for any supported locale
                            bool alreadyLocalized = false;
                            if (node.Locale != SiteConfig.DefaultLocale)
                            {
                                foreach (var loc in SiteConfig.SupportedLocales)
                                {
                                    if (loc == SiteConfig.DefaultLocale) continue; // default has no prefix
                                    if (updatedHref.StartsWith($"/{loc}/"))
                                    {
                                        alreadyLocalized = true;
                                        break;
                                    }
                                }
                            }

                            if (!alreadyLocalized)
                            {
                                updatedHref = SiteConfig.ApplyLocalePath(updatedHref, node.Locale);
                            }
                            else
                            {
                                updatedHref = SiteConfig.ApplyBasePath(updatedHref);
                            }
                        }

                        if (updatedHref != href)
                        {
                            linkNode.SetAttributeValue("href", updatedHref);
                        }
                    }
                }
            }

            // Update the body with processed HTML
            parsed.Body = doc.DocumentNode.OuterHtml;

            return parsed;
        }

        public static async Task<RenderedContent> FromEcmaXmlAsync (ContentNode item, IConfigurationRoot config)
        {
            var content = item.Source.ReadContentAsync ();
            var parsed = new RenderedContent
            {
                Node = item,
                Body = content.Result
            };

            var rendered = EcmaXmlRenderer.Render (item, content.Result, config);

            return await rendered;
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

        public static async Task<string> ToRazorAsync (EcmaXmlContent<EcmaXml.Namespace> nscontent)
        {
            var body = await razorRenderer.RenderContentAsync (nscontent);
            return body;
        }

        public static async Task<string> ToRazorAsync (EcmaXmlContent<EcmaXml.XType> nscontent)
        {
            var body = await razorRenderer.RenderContentAsync (nscontent);
            return body;
        }
    }
}
