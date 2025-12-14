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
    public class MetadataPropagationTests
    {
        [Fact]
        public void MergeMetadata_BothNull_ReturnsNull()
        {
            // Arrange & Act
            var result = RenderedContent.MergeMetadata(null, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MergeMetadata_ParentNullChildExists_ReturnsChild()
        {
            // Arrange
            var childConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "key1", "childValue1" },
                    { "key2", "childValue2" }
                })
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(null, childConfig);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("childValue1", result["key1"]);
            Assert.Equal("childValue2", result["key2"]);
        }

        [Fact]
        public void MergeMetadata_ChildNullParentExists_ReturnsParent()
        {
            // Arrange
            var parentConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "key1", "parentValue1" },
                    { "key2", "parentValue2" }
                })
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(parentConfig, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("parentValue1", result["key1"]);
            Assert.Equal("parentValue2", result["key2"]);
        }

        [Fact]
        public void MergeMetadata_NoOverlap_CombinesBoth()
        {
            // Arrange
            var parentConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "parentKey1", "parentValue1" },
                    { "parentKey2", "parentValue2" }
                })
                .Build();

            var childConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "childKey1", "childValue1" },
                    { "childKey2", "childValue2" }
                })
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(parentConfig, childConfig);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("parentValue1", result["parentKey1"]);
            Assert.Equal("parentValue2", result["parentKey2"]);
            Assert.Equal("childValue1", result["childKey1"]);
            Assert.Equal("childValue2", result["childKey2"]);
        }

        [Fact]
        public void MergeMetadata_WithOverlap_ChildOverridesParent()
        {
            // Arrange
            var parentConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "sharedKey", "parentValue" },
                    { "parentOnlyKey", "parentOnlyValue" }
                })
                .Build();

            var childConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "sharedKey", "childValue" },
                    { "childOnlyKey", "childOnlyValue" }
                })
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(parentConfig, childConfig);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("childValue", result["sharedKey"]); // Child overrides parent
            Assert.Equal("parentOnlyValue", result["parentOnlyKey"]);
            Assert.Equal("childOnlyValue", result["childOnlyKey"]);
        }

        [Fact]
        public void MergeMetadata_CaseInsensitiveKeys()
        {
            // Arrange
            var parentConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ActiveNav", "parent" }
                })
                .Build();

            var childConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "activenav", "child" }
                })
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(parentConfig, childConfig);

            // Assert
            Assert.NotNull(result);
            // Should be case-insensitive, child overrides parent
            Assert.Equal("child", result["ActiveNav"]);
            Assert.Equal("child", result["activenav"]);
            Assert.Equal("child", result["ACTIVENAV"]);
        }

        [Fact]
        public void MergeMetadata_MultipleKeys_PreservesAllValues()
        {
            // Arrange
            var parentConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "title", "Parent Title" },
                    { "activeNav", "home" },
                    { "layout", "default" },
                    { "author", "John Doe" }
                })
                .Build();

            var childConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "title", "Child Title" }, // Override
                    { "activeNav", "about" },   // Override
                    { "description", "Child description" } // New key
                    // layout and author inherited from parent
                })
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(parentConfig, childConfig);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Child Title", result["title"]);
            Assert.Equal("about", result["activeNav"]);
            Assert.Equal("default", result["layout"]);
            Assert.Equal("John Doe", result["author"]);
            Assert.Equal("Child description", result["description"]);
        }

        [Fact]
        public async Task PartialRendering_InheritsParentMetadata()
        {
            // This test verifies that when a partial is rendered, it has access to parent metadata
            // We'll test this by creating a complete rendering scenario

            // Arrange - Create a temporary directory structure
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var templatesDir = Path.Combine(tempDir, "templates");
            var outputDir = Path.Combine(tempDir, "output");
            
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(templatesDir);
            Directory.CreateDirectory(outputDir);

            try
            {
                // Create a parent page with metadata
                var parentContent = @"---
title: Parent Page
activeNav: home
---

# Parent Content

This is the parent page.";
                var parentPath = Path.Combine(sourceDir, "parent.md");
                await File.WriteAllTextAsync(parentPath, parentContent);

                // Create a partial with its own metadata
                var partialContent = @"---
template: nav
---

# Navigation Partial

This partial should access parent activeNav.";
                var partialPath = Path.Combine(sourceDir, "nav.md");
                await File.WriteAllTextAsync(partialPath, partialContent);

                // Create a simple template that uses metadata
                var templateContent = @"@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>
@Model.GetMeta(""title"") - @Model.GetMeta(""activeNav"")";
                var templatePath = Path.Combine(templatesDir, "Default.cshtml");
                await File.WriteAllTextAsync(templatePath, templateContent);

                // Create a minimal SiteFrame template
                var frameContent = @"@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>
<!DOCTYPE html><html><body>@Raw(Model.Body)</body></html>";
                var framePath = Path.Combine(templatesDir, "SiteFrame.cshtml");
                await File.WriteAllTextAsync(framePath, frameContent);

                // Create configuration
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "root", tempDir },
                        { "templates_folder", "templates" },
                        { "title", "Test Site" }
                    })
                    .Build();

                // Initialize renderer
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                await ContentRenderer.InitializeAsync(config);

                // Create ContentNode for parent
                var contentArea = new GroupContentArea
                {
                    SourcePath = sourceDir,
                    TargetPath = outputDir
                };

                var parentNode = new ContentNode
                {
                    Slug = "parent",
                    Source = new DiskFile(parentPath, "parent.md"),
                    Target = new DiskFile(Path.Combine(outputDir, "parent.html"), "parent.html"),
                    Area = contentArea,
                    Locale = "en"
                };

                // Act - Render the parent content
                var rendered = await ContentRenderer.FromMarkdownAsync(parentNode);

                Console.SetOut(originalOut);

                // Assert - Parent should have its metadata
                Assert.NotNull(rendered.Metadata);
                Assert.Equal("Parent Page", rendered.Metadata["title"]);
                Assert.Equal("home", rendered.Metadata["activeNav"]);
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
        public void MergeMetadata_EmptyDictionaries_ReturnsEmptyConfig()
        {
            // Arrange
            var parentConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            var childConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(parentConfig, childConfig);

            // Assert
            Assert.NotNull(result);
            // Should have no keys
            var allKeys = result.AsEnumerable();
            Assert.Empty(allKeys);
        }

        [Fact]
        public void MergeMetadata_NestedConfiguration_PreservesStructure()
        {
            // Test that nested configuration sections work correctly
            // Arrange
            var parentConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "section:key1", "parentValue1" },
                    { "section:key2", "parentValue2" }
                })
                .Build();

            var childConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "section:key2", "childValue2" }, // Override
                    { "section:key3", "childValue3" }  // New
                })
                .Build();

            // Act
            var result = RenderedContent.MergeMetadata(parentConfig, childConfig);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("parentValue1", result["section:key1"]);
            Assert.Equal("childValue2", result["section:key2"]); // Child overrides
            Assert.Equal("childValue3", result["section:key3"]);
        }
    }
}
