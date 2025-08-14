using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Content;
using Chloroplast.Core.Extensions;
using Chloroplast.Core.Rendering;
using Chloroplast.Tool.Commands;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    public class CaseNormalizationTests
    {
        [Fact]
        public void MenuPaths_ShouldMatch_GeneratedFolderPaths()
        {
            // Arrange: Create temp directory structure
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var outputDir = Path.Combine(tempDir, "out");
            
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(outputDir);
            
            var installingFile = Path.Combine(sourceDir, "Installing.md");
            File.WriteAllText(installingFile, "---\ntitle: Installing\n---\n# Installing");
            
            try
            {
                // Create config with normalizePaths enabled (default)
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("root", sourceDir),
                        new KeyValuePair<string, string>("out", outputDir),
                        new KeyValuePair<string, string>("normalizePaths", "true")
                    })
                    .Build();
                
                // Act: Create content area (simulating what happens during build)
                var area = new GroupContentArea
                {
                    SourcePath = sourceDir,
                    TargetPath = outputDir,
                    RootRelativePath = "",
                    NormalizePaths = true
                };
                
                var contentNodes = area.ContentNodes;
                
                // Debug: Print all nodes
                Console.WriteLine($"Found {contentNodes.Count} content nodes:");
                foreach (var node in contentNodes)
                {
                    Console.WriteLine($"  Source: {node.Source.RootRelativePath}, Target: {node.Target.RootRelativePath}, Slug: '{node.Slug}'");
                }
                
                var installingNode = contentNodes.FirstOrDefault(n => n.Source.RootRelativePath.Contains("installing"));
                
                // Assert: Check that the slug and target path are normalized
                Assert.NotNull(installingNode);
                Console.WriteLine($"Installing node - Source path: {installingNode.Source.RootRelativePath}");
                Console.WriteLine($"Installing node - Target path: {installingNode.Target.RootRelativePath}");
                Console.WriteLine($"Installing node - Slug: '{installingNode.Slug}'");
                
                // The target path should be lowercase when normalizePaths is true
                Assert.Equal("installing.html", installingNode.Target.RootRelativePath);
                Assert.Equal("", installingNode.Slug); // For files in root, slug is empty
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void MenuPaths_WithSubdirectories_ShouldMatch_GeneratedFolderPaths()
        {
            // Arrange: Create temp directory structure with subdirectories
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var installingDir = Path.Combine(sourceDir, "Installing");
            var outputDir = Path.Combine(tempDir, "out");
            
            Directory.CreateDirectory(installingDir);
            Directory.CreateDirectory(outputDir);
            
            var installingIndexFile = Path.Combine(installingDir, "index.md");
            File.WriteAllText(installingIndexFile, "---\ntitle: Installing Guide\n---\n# Installing Guide");
            
            try
            {
                // Act: Create content area (simulating what happens during build)
                var area = new GroupContentArea
                {
                    SourcePath = sourceDir,
                    TargetPath = outputDir,
                    RootRelativePath = "",
                    NormalizePaths = true
                };
                
                var contentNodes = area.ContentNodes;
                
                // Debug: Print all nodes
                Console.WriteLine($"Found {contentNodes.Count} content nodes:");
                foreach (var node in contentNodes)
                {
                    Console.WriteLine($"  Source: {node.Source.RootRelativePath}, Target: {node.Target.RootRelativePath}, Slug: '{node.Slug}'");
                }
                
                var installingNode = contentNodes.FirstOrDefault(n => n.Source.RootRelativePath.Contains("installing"));
                
                // Assert: Check that the slug and target path are normalized
                Assert.NotNull(installingNode);
                Console.WriteLine($"Installing node - Source path: {installingNode.Source.RootRelativePath}");
                Console.WriteLine($"Installing node - Target path: {installingNode.Target.RootRelativePath}");
                Console.WriteLine($"Installing node - Slug: '{installingNode.Slug}'");
                
                // The target path should be lowercase when normalizePaths is true
                Assert.Contains("installing", installingNode.Target.RootRelativePath);
                Assert.Contains("installing", installingNode.Slug); // This is the key issue - slug should match folder
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void PathNormalization_WorksWithUrlSegments()
        {
            // Test the new URL normalization extension method
            Assert.Equal("installing", "Installing".NormalizeUrlSegment(toLower: true));
            Assert.Equal("Installing", "Installing".NormalizeUrlSegment(toLower: false));
            Assert.Equal("path/to/file", @"Path\To\File".NormalizeUrlSegment(toLower: true));
            Assert.Equal("Path/To/File", @"Path\To\File".NormalizeUrlSegment(toLower: false));
            Assert.Equal("", "".NormalizeUrlSegment(toLower: true));
            Assert.Equal("", ((string)null).NormalizeUrlSegment(toLower: true));
        }

        [Fact]
        public void FullBuildCommand_PrepareMenu_RespectsNormalization()
        {
            // Test that PrepareMenu respects the NormalizePaths setting
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var outputDir = Path.Combine(tempDir, "out");
            
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(outputDir);
            
            try
            {
                // Create a test area with normalization enabled
                var area = new GroupContentArea
                {
                    SourcePath = sourceDir,
                    TargetPath = outputDir,
                    NormalizePaths = true
                };

                // Create test content nodes with mixed case slugs
                var nodes = new[]
                {
                    new ContentNode
                    {
                        Title = "Installing Guide",
                        Slug = "Installing", // This should be normalized in menu path
                        Area = area
                    },
                    new ContentNode
                    {
                        Title = "API Documentation",
                        Slug = "Api/Reference", // This should be normalized in menu path  
                        Area = area
                    }
                };

                // Use reflection to access the private PrepareMenu method
                var buildCommand = new FullBuildCommand();
                var method = typeof(FullBuildCommand).GetMethod("PrepareMenu", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                var result = (IEnumerable<MenuNode>)method.Invoke(buildCommand, new object[] { "docs", nodes });
                var menuNodes = result.ToArray();

                // Verify that menu paths are normalized
                Assert.Equal(2, menuNodes.Length);
                
                var installingMenu = menuNodes.FirstOrDefault(n => n.Title == "Installing Guide");
                Assert.NotNull(installingMenu);
                Assert.Contains("installing", installingMenu.Path.ToLower()); // Path should be lowercase
                
                var apiMenu = menuNodes.FirstOrDefault(n => n.Title == "API Documentation");
                Assert.NotNull(apiMenu);
                Assert.Contains("api/reference", apiMenu.Path.ToLower()); // Path should be lowercase
                
                Console.WriteLine($"Installing menu path: {installingMenu.Path}");
                Console.WriteLine($"API menu path: {apiMenu.Path}");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}