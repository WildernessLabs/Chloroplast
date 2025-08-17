using System.Collections.Generic;
using Chloroplast.Core;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    public class LocalizationTests
    {
        private IConfigurationRoot MakeConfig(Dictionary<string, string> configValues)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(configValues);
            var cfg = builder.Build();
            SiteConfig.Instance = cfg;
            return cfg;
        }

        [Fact]
        public void DefaultLocale_ReturnsDefaultValue_WhenNotConfigured()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>());

            // Act
            var defaultLocale = SiteConfig.DefaultLocale;

            // Assert
            Assert.Equal("en", defaultLocale);
        }

        [Fact]
        public void DefaultLocale_ReturnsConfiguredValue()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["defaultLocale"] = "es"
            });

            // Act
            var defaultLocale = SiteConfig.DefaultLocale;

            // Assert
            Assert.Equal("es", defaultLocale);
        }

        [Fact]
        public void SupportedLocales_ReturnsDefaultOnly_WhenNotConfigured()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["defaultLocale"] = "fr"
            });

            // Act
            var supportedLocales = SiteConfig.SupportedLocales;

            // Assert
            Assert.Single(supportedLocales);
            Assert.Equal("fr", supportedLocales[0]);
        }

        [Fact]
        public void SupportedLocales_ReturnsConfiguredValues()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["defaultLocale"] = "en",
                ["supportedLocales:0"] = "en",
                ["supportedLocales:1"] = "es",
                ["supportedLocales:2"] = "fr"
            });

            // Act
            var supportedLocales = SiteConfig.SupportedLocales;

            // Assert
            Assert.Equal(3, supportedLocales.Length);
            Assert.Contains("en", supportedLocales);
            Assert.Contains("es", supportedLocales);
            Assert.Contains("fr", supportedLocales);
        }

        [Fact]
        public void ApplyLocalePath_ReturnsBasePath_ForDefaultLocale()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = "/Chloroplast",
                ["defaultLocale"] = "en"
            });

            // Act
            var result = SiteConfig.ApplyLocalePath("/docs/guide", "en");

            // Assert
            Assert.Equal("/Chloroplast/docs/guide", result);
        }

        [Fact]
        public void ApplyLocalePath_ReturnsLocalizedPath_ForNonDefaultLocale()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = "/Chloroplast",
                ["defaultLocale"] = "en"
            });

            // Act
            var result = SiteConfig.ApplyLocalePath("/docs/guide", "es");

            // Assert
            Assert.Equal("/Chloroplast/es/docs/guide", result);
        }

        [Fact]
        public void ApplyLocalePath_HandlesRootPath_ForNonDefaultLocale()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = "/Chloroplast",
                ["defaultLocale"] = "en"
            });

            // Act
            var result = SiteConfig.ApplyLocalePath("/", "fr");

            // Assert
            Assert.Equal("/Chloroplast/fr/", result);
        }

        [Fact]
        public void ApplyLocalePath_HandlesEmptyPath_ForNonDefaultLocale()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["basePath"] = "/Chloroplast",
                ["defaultLocale"] = "en"
            });

            // Act
            var result = SiteConfig.ApplyLocalePath("", "es");

            // Assert
            Assert.Equal("/Chloroplast/es", result);
        }

        [Fact]
        public void ApplyLocalePath_PreservesAbsoluteUrls()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["defaultLocale"] = "en"
            });

            // Act
            var result = SiteConfig.ApplyLocalePath("https://external.com/path", "es");

            // Assert
            Assert.Equal("https://external.com/path", result);
        }

        [Fact]
        public void ApplyLocalePath_PreservesFragments()
        {
            // Arrange
            MakeConfig(new Dictionary<string, string>
            {
                ["defaultLocale"] = "en"
            });

            // Act
            var result = SiteConfig.ApplyLocalePath("#section", "es");

            // Assert
            Assert.Equal("#section", result);
        }

        [Fact]
        public void ContentNode_HasLocaleProperty()
        {
            // Arrange & Act
            var node = new ContentNode
            {
                Locale = "es",
                Slug = "test"
            };

            // Assert
            Assert.Equal("es", node.Locale);
            Assert.NotNull(node.Translations);
            Assert.Empty(node.Translations);
        }

        [Fact]
        public void ContentNode_ToString_IncludesLocale()
        {
            // Arrange
            var node = new ContentNode
            {
                Locale = "fr",
                Slug = "test-slug",
                Title = "Test Title"
            };

            // Act
            var result = node.ToString();

            // Assert
            Assert.Contains("(fr)", result);
            Assert.Contains("test-slug", result);
            Assert.Contains("Test Title", result);
        }
    }
}