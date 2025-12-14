using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Content;
using Chloroplast.Core.Rendering;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    public class FrameSelectionTests
    {
        [Fact]
        public async Task DefaultFrameIsUsed_WhenNoFrameSpecified()
        {
            // Arrange - Create a temporary directory structure with templates
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            // Create SiteFrame template
            var siteFrameContent = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\nSiteFrame: @Model.Body";
            var siteFramePath = Path.Combine(templatesDir, "SiteFrame.cshtml");
            await File.WriteAllTextAsync(siteFramePath, siteFrameContent);
            
            // Create configuration
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "root", tempDir },
                    { "templates_folder", "templates" }
                })
                .Build();
            
            var renderer = new RazorRenderer();
            
            try
            {
                // Suppress console output
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                // Act
                await renderer.InitializeAsync(config);
                
                // Create test content without frame metadata
                var content = new FrameRenderedContent(
                    new RenderedContent 
                    { 
                        Body = "<p>Test content</p>",
                        Metadata = new ConfigurationBuilder().Build(),
                        Node = new ContentNode { Title = "Test" }
                    },
                    new List<ContentNode>()
                );
                
                var result = await renderer.RenderContentAsync(content);
                
                Console.SetOut(originalOut);
                
                // Assert
                Assert.NotNull(result);
                Assert.Contains("SiteFrame:", result);
                Assert.Contains("Test content", result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public async Task CustomFrameIsUsed_WhenFrameSpecified()
        {
            // Arrange - Create templates
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            // Create SiteFrame and CustomFrame templates
            var siteFrameContent = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\nSiteFrame: @Model.Body";
            var siteFramePath = Path.Combine(templatesDir, "SiteFrame.cshtml");
            await File.WriteAllTextAsync(siteFramePath, siteFrameContent);
            
            var customFrameContent = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\nCustomFrame: @Model.Body";
            var customFramePath = Path.Combine(templatesDir, "CustomFrame.cshtml");
            await File.WriteAllTextAsync(customFramePath, customFrameContent);
            
            // Create configuration
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "root", tempDir },
                    { "templates_folder", "templates" }
                })
                .Build();
            
            var renderer = new RazorRenderer();
            
            try
            {
                // Suppress console output
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                // Act
                await renderer.InitializeAsync(config);
                
                // Create test content with custom frame metadata
                var metadata = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "frame", "CustomFrame" }
                    })
                    .Build();
                
                var content = new FrameRenderedContent(
                    new RenderedContent 
                    { 
                        Body = "<p>Test content with custom frame</p>",
                        Metadata = metadata,
                        Node = new ContentNode { Title = "Test" }
                    },
                    new List<ContentNode>()
                );
                
                var result = await renderer.RenderContentAsync(content);
                
                Console.SetOut(originalOut);
                
                // Assert
                Assert.NotNull(result);
                Assert.Contains("CustomFrame:", result);
                Assert.Contains("Test content with custom frame", result);
                Assert.DoesNotContain("SiteFrame:", result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public async Task MissingFrame_ReturnsNull_AndLogsError()
        {
            // Arrange - Create templates but not the requested one
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            // Create only SiteFrame, not NonExistentFrame
            var siteFrameContent = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\nSiteFrame: @Model.Body";
            var siteFramePath = Path.Combine(templatesDir, "SiteFrame.cshtml");
            await File.WriteAllTextAsync(siteFramePath, siteFrameContent);
            
            // Create configuration
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "root", tempDir },
                    { "templates_folder", "templates" }
                })
                .Build();
            
            var renderer = new RazorRenderer();
            
            try
            {
                // Capture console output
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                // Act
                await renderer.InitializeAsync(config);
                
                // Create test content with non-existent frame
                var metadata = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "frame", "NonExistentFrame" }
                    })
                    .Build();
                
                var content = new FrameRenderedContent(
                    new RenderedContent 
                    { 
                        Body = "<p>Test content</p>",
                        Metadata = metadata,
                        Node = new ContentNode { Title = "Test Page" }
                    },
                    new List<ContentNode>()
                );
                
                var result = await renderer.RenderContentAsync(content);
                
                Console.SetOut(originalOut);
                var consoleOutput = stringWriter.ToString();
                
                // Assert
                Assert.Null(result); // Should return null when frame is missing
                Assert.Contains("ERROR", consoleOutput);
                Assert.Contains("NonExistentFrame", consoleOutput);
                Assert.Contains("not found", consoleOutput);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public async Task MissingSiteFrame_ReturnsNull_AndLogsError()
        {
            // Arrange - Create templates directory without SiteFrame
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            // Don't create SiteFrame.cshtml
            
            // Create configuration
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "root", tempDir },
                    { "templates_folder", "templates" }
                })
                .Build();
            
            var renderer = new RazorRenderer();
            
            try
            {
                // Capture console output
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                // Act
                await renderer.InitializeAsync(config);
                
                // Create test content without frame metadata (should use default SiteFrame)
                var content = new FrameRenderedContent(
                    new RenderedContent 
                    { 
                        Body = "<p>Test content</p>",
                        Metadata = new ConfigurationBuilder().Build(),
                        Node = new ContentNode { Title = "Test Page" }
                    },
                    new List<ContentNode>()
                );
                
                var result = await renderer.RenderContentAsync(content);
                
                Console.SetOut(originalOut);
                var consoleOutput = stringWriter.ToString();
                
                // Assert
                Assert.Null(result); // Should return null when default frame is missing
                Assert.Contains("ERROR", consoleOutput);
                Assert.Contains("SiteFrame", consoleOutput);
                Assert.Contains("not found", consoleOutput);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
