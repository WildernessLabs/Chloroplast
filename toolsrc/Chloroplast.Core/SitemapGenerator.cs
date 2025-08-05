using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Chloroplast.Core.Content;
using Chloroplast.Core.Extensions;

namespace Chloroplast.Core
{
    public class SitemapGenerator
    {
        private const int DefaultMaxUrlsPerSitemap = 50000;
        private const string SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

        public string BaseUrl { get; set; }
        public int MaxUrlsPerSitemap { get; set; } = DefaultMaxUrlsPerSitemap;
        public string OutputDirectory { get; set; }

        public SitemapGenerator(string baseUrl, string outputDirectory)
        {
            BaseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            OutputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
        }

        /// <summary>
        /// Generates sitemap XML files from content nodes
        /// </summary>
        /// <param name="contentNodes">The content nodes to include in the sitemap</param>
        /// <returns>List of generated sitemap file paths</returns>
        public IEnumerable<string> GenerateSitemaps(IEnumerable<ContentNode> contentNodes)
        {
            var sitemapUrls = ExtractUrls(contentNodes).ToList();
            
            if (!sitemapUrls.Any())
            {
                return Enumerable.Empty<string>();
            }

            var sitemapFiles = new List<string>();

            // If we have fewer URLs than the max, create a single sitemap
            if (sitemapUrls.Count <= MaxUrlsPerSitemap)
            {
                var sitemapPath = Path.Combine(OutputDirectory, "sitemap.xml");
                GenerateSingleSitemap(sitemapUrls, sitemapPath);
                sitemapFiles.Add(sitemapPath);
            }
            else
            {
                // Split into multiple sitemaps and create a sitemap index
                var chunks = ChunkUrls(sitemapUrls, MaxUrlsPerSitemap);
                var sitemapPaths = new List<string>();

                for (int i = 0; i < chunks.Count; i++)
                {
                    var sitemapPath = Path.Combine(OutputDirectory, $"sitemap{i + 1}.xml");
                    GenerateSingleSitemap(chunks[i], sitemapPath);
                    sitemapPaths.Add(sitemapPath);
                    sitemapFiles.Add(sitemapPath);
                }

                // Create sitemap index
                var indexPath = Path.Combine(OutputDirectory, "sitemap.xml");
                GenerateSitemapIndex(sitemapPaths, indexPath);
                sitemapFiles.Add(indexPath);
            }

            return sitemapFiles;
        }

        private IEnumerable<SitemapUrl> ExtractUrls(IEnumerable<ContentNode> contentNodes)
        {
            foreach (var node in contentNodes)
            {
                if (ShouldIncludeInSitemap(node))
                {
                    var url = BuildUrlFromNode(node);
                    if (!string.IsNullOrEmpty(url))
                    {
                        yield return new SitemapUrl 
                        { 
                            Location = url,
                            LastModified = node.Source.LastUpdated
                        };
                    }
                }
            }
        }

        private bool ShouldIncludeInSitemap(ContentNode node)
        {
            // Include HTML files, exclude other assets like CSS, images etc.
            return node.Target.RootRelativePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
        }

        private string BuildUrlFromNode(ContentNode node)
        {
            var relativePath = node.Target.RootRelativePath.Replace('\\', '/');
            
            // Remove leading slash if present
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }

            return $"{BaseUrl}/{relativePath}";
        }

        private List<List<SitemapUrl>> ChunkUrls(List<SitemapUrl> urls, int chunkSize)
        {
            var chunks = new List<List<SitemapUrl>>();
            for (int i = 0; i < urls.Count; i += chunkSize)
            {
                chunks.Add(urls.Skip(i).Take(chunkSize).ToList());
            }
            return chunks;
        }

        private void GenerateSingleSitemap(IEnumerable<SitemapUrl> urls, string filePath)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", SitemapNamespace);

                foreach (var url in urls)
                {
                    writer.WriteStartElement("url");
                    
                    writer.WriteElementString("loc", url.Location);
                    
                    if (url.LastModified.HasValue)
                    {
                        writer.WriteElementString("lastmod", url.LastModified.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    }
                    
                    writer.WriteEndElement(); // url
                }

                writer.WriteEndElement(); // urlset
                writer.WriteEndDocument();
            }
        }

        private void GenerateSitemapIndex(IEnumerable<string> sitemapPaths, string indexPath)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(indexPath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("sitemapindex", SitemapNamespace);

                foreach (var sitemapPath in sitemapPaths)
                {
                    var fileName = Path.GetFileName(sitemapPath);
                    var sitemapUrl = $"{BaseUrl}/{fileName}";

                    writer.WriteStartElement("sitemap");
                    writer.WriteElementString("loc", sitemapUrl);
                    
                    // Use current time as lastmod for sitemap index
                    writer.WriteElementString("lastmod", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    
                    writer.WriteEndElement(); // sitemap
                }

                writer.WriteEndElement(); // sitemapindex
                writer.WriteEndDocument();
            }
        }
    }

    public class SitemapUrl
    {
        public string Location { get; set; }
        public DateTime? LastModified { get; set; }
    }
}