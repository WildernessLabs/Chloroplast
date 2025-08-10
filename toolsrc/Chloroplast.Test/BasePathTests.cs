using System.Collections.Generic;
using Chloroplast.Core;
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
    }
}
