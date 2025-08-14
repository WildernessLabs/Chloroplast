using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chloroplast.Core.Content;
using Chloroplast.Core.Extensions;
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
    }
}