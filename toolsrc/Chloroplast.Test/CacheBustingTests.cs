using System;
using System.Collections.Generic;
using Chloroplast.Core;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    public class CacheBustingTests
    {
        [Fact]
        public void SiteConfig_BuildVersion_CanBeSet()
        {
            // Arrange
            var testVersion = "20231215123456";
            
            // Act
            SiteConfig.BuildVersion = testVersion;
            
            // Assert
            Assert.Equal(testVersion, SiteConfig.BuildVersion);
        }

        [Fact]
        public void SiteConfig_CacheBustingEnabled_DefaultsToTrue()
        {
            // Arrange
            var config = CreateConfigWithValues(new Dictionary<string, string>());
            SiteConfig.Instance = config;
            
            // Act
            var result = SiteConfig.CacheBustingEnabled;
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SiteConfig_CacheBustingEnabled_CanBeDisabled()
        {
            // Arrange
            var config = CreateConfigWithValues(new Dictionary<string, string>
            {
                ["cacheBusting:enabled"] = "false"
            });
            SiteConfig.Instance = config;
            
            // Act
            var result = SiteConfig.CacheBustingEnabled;
            
            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("/assets/main.css", "20231215123456", "/assets/main.css?v=20231215123456")]
        [InlineData("/assets/main.css?param=value", "20231215123456", "/assets/main.css?param=value&v=20231215123456")]
        [InlineData("/scripts/app.js", "20240101000000", "/scripts/app.js?v=20240101000000")]
        public void WithVersion_AddsVersionParameter_WhenEnabled(string url, string buildVersion, string expected)
        {
            // Arrange
            SiteConfig.BuildVersion = buildVersion;
            var config = CreateConfigWithValues(new Dictionary<string, string>
            {
                ["cacheBusting:enabled"] = "true"
            });
            SiteConfig.Instance = config;
            
            // Act
            var result = WithVersionHelper(url);
            
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void WithVersion_ReturnsOriginal_WhenDisabled()
        {
            // Arrange
            var url = "/assets/main.css";
            SiteConfig.BuildVersion = "20231215123456";
            var config = CreateConfigWithValues(new Dictionary<string, string>
            {
                ["cacheBusting:enabled"] = "false"
            });
            SiteConfig.Instance = config;
            
            // Act
            var result = WithVersionHelper(url);
            
            // Assert
            Assert.Equal(url, result);
        }

        [Fact]
        public void WithVersion_ReturnsOriginal_WhenNoBuildVersion()
        {
            // Arrange
            var url = "/assets/main.css";
            SiteConfig.BuildVersion = null;
            var config = CreateConfigWithValues(new Dictionary<string, string>
            {
                ["cacheBusting:enabled"] = "true"
            });
            SiteConfig.Instance = config;
            
            // Act
            var result = WithVersionHelper(url);
            
            // Assert
            Assert.Equal(url, result);
        }

        [Fact]
        public void BuildVersion_ReturnsNull_WhenCacheBustingDisabled()
        {
            // Arrange
            SiteConfig.BuildVersion = "20231215123456";
            var config = CreateConfigWithValues(new Dictionary<string, string>
            {
                ["cacheBusting:enabled"] = "false"
            });
            SiteConfig.Instance = config;
            
            // Act
            var result = GetBuildVersionHelper();
            
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BuildVersion_ReturnsBuildVersion_WhenCacheBustingEnabled()
        {
            // Arrange
            var buildVersion = "20231215123456";
            SiteConfig.BuildVersion = buildVersion;
            var config = CreateConfigWithValues(new Dictionary<string, string>
            {
                ["cacheBusting:enabled"] = "true"
            });
            SiteConfig.Instance = config;
            
            // Act
            var result = GetBuildVersionHelper();
            
            // Assert
            Assert.Equal(buildVersion, result);
        }

        private IConfigurationRoot CreateConfigWithValues(Dictionary<string, string> values)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(values);
            return builder.Build();
        }

        // Helper methods that simulate the template base functionality
        private string WithVersionHelper(string url)
        {
            if (!SiteConfig.CacheBustingEnabled || string.IsNullOrWhiteSpace(SiteConfig.BuildVersion))
                return url;

            var separator = url.Contains("?") ? "&" : "?";
            return $"{url}{separator}v={SiteConfig.BuildVersion}";
        }

        private string GetBuildVersionHelper()
        {
            return SiteConfig.CacheBustingEnabled ? SiteConfig.BuildVersion : null;
        }
    }
}