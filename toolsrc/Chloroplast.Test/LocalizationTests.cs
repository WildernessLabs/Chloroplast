using System.Collections.Generic;
using System.Threading.Tasks;
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
            // Ensure tests always start with base path enabled (not overridden by CLI flag in prior runs)
            SiteConfig.DisableBasePath = false;
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
        
        [Theory]
        [InlineData("en", "🇺🇸")]
        [InlineData("es", "🇪🇸")]
        [InlineData("fr", "🌐")]  // Falls back to generic globe
        [InlineData("de", "🌐")]  // Falls back to generic globe
        [InlineData("unknown", "🌐")]
        public void GetCountryFlag_ReturnsCorrectFlag(string locale, string expectedFlag)
        {
            // Arrange
            var template = new TestTemplate();

            // Act
            var result = template.TestGetCountryFlag(locale);

            // Assert
            Assert.Equal(expectedFlag, result);
        }
        
        [Theory]
        [InlineData("en", "English")]
        [InlineData("es", "Español")]
        [InlineData("fr", "FR")]  // Falls back to uppercase
        [InlineData("de", "DE")]  // Falls back to uppercase
        [InlineData("unknown", "UNKNOWN")]
        public void GetLocaleDisplayName_ReturnsCorrectName(string locale, string expectedName)
        {
            // Arrange
            var template = new TestTemplate();

            // Act
            var result = template.TestGetLocaleDisplayName(locale);

            // Assert
            Assert.Equal(expectedName, result);
        }
        
        private class TestTemplate : Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>
        {
            public string TestGetCountryFlag(string locale) => GetCountryFlag(locale);
            public string TestGetLocaleDisplayName(string locale) => GetLocaleDisplayName(locale);
            
            public override Task ExecuteAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}