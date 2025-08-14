using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Content;
using Chloroplast.Core.Rendering;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    public class BasePathTests
    {
        private IConfigurationRoot MakeConfig(Dictionary<string, string> values)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(values);
            var cfg = builder.Build();
            SiteConfig.Instance = cfg;
            return cfg;
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("/", "")]
        [InlineData("blah", "/blah")]
        [InlineData("blah/", "/blah")]
        [InlineData("/blah", "/blah")]
        [InlineData("/blah/", "/blah")]
        public void BasePath_IsNormalized(string input, string expected)
        {
            var dict = new Dictionary<string, string>();
            if (input != null)
                dict["basePath"] = input;

            MakeConfig(dict);

            Assert.Equal(expected, SiteConfig.BasePath);
        }

        [Theory]
        [InlineData("https://example.com", "")]          // no path component
        [InlineData("https://example.com/", "")]         // root path
        [InlineData("https://example.com/repo", "/repo")]
        [InlineData("https://example.com/repo/", "/repo")]
        [InlineData("https://example.com/owner/repo", "/owner/repo")]
        public void BasePath_Derives_From_BaseUrl_When_Not_Specified(string baseUrl, string expected)
        {
            MakeConfig(new Dictionary<string, string>
            {
                ["baseUrl"] = baseUrl
            });

            Assert.Equal(expected, SiteConfig.BasePath);
        }

        [Fact]
        public void BasePath_Prefers_Explicit_When_Both_Set()
        {
            MakeConfig(new Dictionary<string, string>
            {
                ["baseUrl"] = "https://example.com/owner/repo",
                ["basePath"] = "/custom"
            });

            Assert.Equal("/custom", SiteConfig.BasePath);
        }

        [Theory]
        [InlineData("/assets/site.css", "/blah", "/blah/assets/site.css")]
        [InlineData("assets/site.css", "/blah", "/blah/assets/site.css")]
        [InlineData("/assets/site.css", "", "/assets/site.css")]
        [InlineData("/", "/base", "/base/")]
        [InlineData("#section", "/base", "#section")]
        [InlineData("https://example.com/x.css", "/base", "https://example.com/x.css")]
        [InlineData("/assets/site.css", "/base", "/base/assets/site.css")]
        public void ApplyBasePath_PrefixesCorrectly(string input, string basePath, string expected)
        {
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = basePath
            });

            Assert.Equal(expected, SiteConfig.ApplyBasePath(input));
        }

        [Theory]
        [InlineData("blah", "/blah/assets/main.css")] // ensure leading slash inserted
        [InlineData("/blah/", "/blah/assets/main.css")] // trailing removed
        public void ApplyBasePath_On_Assets_Avoids_DoubleSlashes(string basePath, string expected)
        {
            // Configure
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = basePath
            });

            var url = SiteConfig.ApplyBasePath("/assets/main.css");
            Assert.Equal(expected, url);
        }

        [Fact]
        public async Task ContentRenderer_AppliesBasePath_ToRootRelativeLinks()
        {
            // Configure with base path
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = "/Chloroplast"
            });

            // Create markdown content with root relative links
            var markdown = @"---
title: Test Page
---

# Test Content

You can [install it](/Installing) and [use it](/cli). Visit our [home page](/) too.

Also see [external link](https://github.com/example) and [relative link](../other) and [fragment link](#section).
";

            // Create a mock content node
            var mockSource = new MockContentSource(markdown);
            var contentNode = new ContentNode
            {
                Source = mockSource,
                Slug = "test",
                Title = "Test"
            };

            // Process the content
            var result = await ContentRenderer.FromMarkdownAsync(contentNode);

            // Verify root relative links are prefixed with BasePath
            Assert.Contains("href=\"/Chloroplast/Installing\"", result.Body);
            Assert.Contains("href=\"/Chloroplast/cli\"", result.Body);
            Assert.Contains("href=\"/Chloroplast/\"", result.Body);

            // Verify non-root-relative links are not modified
            Assert.Contains("href=\"https://github.com/example\"", result.Body);
            Assert.Contains("href=\"../other\"", result.Body);
            Assert.Contains("href=\"#section\"", result.Body);
        }

        [Fact]
        public async Task ContentRenderer_DoesNotModifyLinks_WhenNoBasePath()
        {
            // Configure with no base path
            MakeConfig(new Dictionary<string, string>());

            var markdown = @"---
title: Test Page
---

Visit [our docs](/docs) and [home page](/).
";

            var mockSource = new MockContentSource(markdown);
            var contentNode = new ContentNode
            {
                Source = mockSource,
                Slug = "test",
                Title = "Test"
            };

            var result = await ContentRenderer.FromMarkdownAsync(contentNode);

            // Verify links remain unchanged when no base path
            Assert.Contains("href=\"/docs\"", result.Body);
            Assert.Contains("href=\"/\"", result.Body);
        }

        [Fact]
        public async Task ContentRenderer_HandlesComplexMarkdownLinks()
        {
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = "/mysite"
            });

            var markdown = @"---
title: Complex Links
---

# Links Test

- [Simple link](/page1)
- [Link with title](/page2 ""Page 2 Title"")
- [Complex path](/some/deep/path/)
- Reference style [link][1]

[1]: /reference-target

Regular paragraph with [inline link](/inline) in text.
";

            var mockSource = new MockContentSource(markdown);
            var contentNode = new ContentNode
            {
                Source = mockSource,
                Slug = "test",
                Title = "Test"
            };

            var result = await ContentRenderer.FromMarkdownAsync(contentNode);

            // Verify all root relative links get base path
            Assert.Contains("href=\"/mysite/page1\"", result.Body);
            Assert.Contains("href=\"/mysite/page2\"", result.Body);
            Assert.Contains("href=\"/mysite/some/deep/path/\"", result.Body);
            Assert.Contains("href=\"/mysite/reference-target\"", result.Body);
            Assert.Contains("href=\"/mysite/inline\"", result.Body);
        }
    }

    // Mock content source for testing
    public class MockContentSource : IFile
    {
        private readonly string _content;

        public MockContentSource(string content)
        {
            _content = content;
        }

        public string RootRelativePath { get; set; } = "/test.md";
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public Task<string> ReadContentAsync()
        {
            return Task.FromResult(_content);
        }

        public void CopyTo(IFile target)
        {
            throw new NotImplementedException();
        }

        public Task WriteContentAsync(string content)
        {
            throw new NotImplementedException();
        }
    }
}
