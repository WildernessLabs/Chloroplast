using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chloroplast.Core;
using Chloroplast.Core.Content;
using Xunit;

namespace Chloroplast.Test
{
    public class SitemapGeneratorTests
    {
        [Fact]
        public void GenerateSitemaps_WithSinglePage_CreatesSingleSitemap()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                var generator = new SitemapGenerator("https://example.com", tempDir);
                var contentNodes = CreateTestContentNodes(1);

                // Act
                var sitemapFiles = generator.GenerateSitemaps(contentNodes).ToList();

                // Assert
                Assert.Single(sitemapFiles);
                var sitemapPath = Path.Combine(tempDir, "sitemap.xml");
                Assert.True(File.Exists(sitemapPath));
                
                // Verify XML content
                var xml = File.ReadAllText(sitemapPath);
                Assert.Contains("https://example.com/test0/index.html", xml);
                Assert.Contains("http://www.sitemaps.org/schemas/sitemap/0.9", xml);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void GenerateSitemaps_WithMultiplePages_UnderLimit_CreatesSingleSitemap()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                var generator = new SitemapGenerator("https://example.com", tempDir);
                var contentNodes = CreateTestContentNodes(10);

                // Act
                var sitemapFiles = generator.GenerateSitemaps(contentNodes).ToList();

                // Assert
                Assert.Single(sitemapFiles);
                var sitemapPath = Path.Combine(tempDir, "sitemap.xml");
                Assert.True(File.Exists(sitemapPath));
                
                // Verify XML contains all URLs
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(sitemapPath);
                var urlNodes = xmlDoc.SelectNodes("//ns:url", CreateNamespaceManager(xmlDoc));
                Assert.Equal(10, urlNodes.Count);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void GenerateSitemaps_ExceedsLimit_CreatesMultipleSitemapsWithIndex()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                var generator = new SitemapGenerator("https://example.com", tempDir)
                {
                    MaxUrlsPerSitemap = 10
                };
                var contentNodes = CreateTestContentNodes(25);

                // Act
                var sitemapFiles = generator.GenerateSitemaps(contentNodes).ToList();

                // Assert
                Assert.Equal(4, sitemapFiles.Count); // 3 sitemaps + 1 index
                Assert.True(File.Exists(Path.Combine(tempDir, "sitemap.xml"))); // Index
                Assert.True(File.Exists(Path.Combine(tempDir, "sitemap1.xml")));
                Assert.True(File.Exists(Path.Combine(tempDir, "sitemap2.xml")));
                Assert.True(File.Exists(Path.Combine(tempDir, "sitemap3.xml")));

                // Verify index contains references to individual sitemaps
                var indexXml = File.ReadAllText(Path.Combine(tempDir, "sitemap.xml"));
                Assert.Contains("sitemap1.xml", indexXml);
                Assert.Contains("sitemap2.xml", indexXml);
                Assert.Contains("sitemap3.xml", indexXml);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void GenerateSitemaps_SkipsNonHtmlFiles()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                var generator = new SitemapGenerator("https://example.com", tempDir);
                var contentNodes = new List<ContentNode>
                {
                    CreateTestContentNode("test1.html"),
                    CreateTestContentNode("style.css"),
                    CreateTestContentNode("image.png"),
                    CreateTestContentNode("test2.html")
                };

                // Act
                var sitemapFiles = generator.GenerateSitemaps(contentNodes).ToList();

                // Assert
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(sitemapFiles.First());
                var urlNodes = xmlDoc.SelectNodes("//ns:url", CreateNamespaceManager(xmlDoc));
                Assert.Equal(2, urlNodes.Count); // Only HTML files
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void GenerateSitemaps_EmptyContentNodes_ReturnsEmptyList()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                var generator = new SitemapGenerator("https://example.com", tempDir);
                var contentNodes = new List<ContentNode>();

                // Act
                var sitemapFiles = generator.GenerateSitemaps(contentNodes).ToList();

                // Assert
                Assert.Empty(sitemapFiles);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Constructor_NullBaseUrl_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SitemapGenerator(null, "/temp"));
        }

        [Fact]
        public void Constructor_NullOutputDirectory_ThrowsException()
        {
            // Act & Assert  
            Assert.Throws<ArgumentNullException>(() => new SitemapGenerator("https://example.com", null));
        }

        private List<ContentNode> CreateTestContentNodes(int count)
        {
            var nodes = new List<ContentNode>();
            for (int i = 0; i < count; i++)
            {
                nodes.Add(CreateTestContentNode($"test{i}/index.html"));
            }
            return nodes;
        }

        private ContentNode CreateTestContentNode(string relativePath)
        {
            var mockSource = new MockFile("source/" + relativePath, relativePath);
            var mockTarget = new MockFile("output/" + relativePath, relativePath);
            
            return new ContentNode
            {
                Source = mockSource,
                Target = mockTarget,
                Title = "Test Page"
            };
        }

        private XmlNamespaceManager CreateNamespaceManager(XmlDocument doc)
        {
            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("ns", "http://www.sitemaps.org/schemas/sitemap/0.9");
            return nsManager;
        }
    }

    // Mock implementation for testing
    public class MockFile : IFile
    {
        public string Path { get; }
        public string RootRelativePath { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public MockFile(string path, string rootRelativePath)
        {
            Path = path;
            RootRelativePath = rootRelativePath;
        }

        public void CopyTo(IFile target) => throw new NotImplementedException();
        public System.Threading.Tasks.Task<string> ReadContentAsync() => throw new NotImplementedException();
        public System.Threading.Tasks.Task WriteContentAsync(string content) => throw new NotImplementedException();
    }
}