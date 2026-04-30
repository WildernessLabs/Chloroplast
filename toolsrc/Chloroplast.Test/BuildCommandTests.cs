using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Extensions;
using Chloroplast.Tool.Commands;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    public class BuildCommandTests
    {
        [Fact]
        public async Task BuildCommand_ShouldClearOutputDirectory_BeforeBuilding()
        {
            // Arrange: Create temp directory structure
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var outputDir = Path.Combine(tempDir, "out");
            
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(outputDir);
            
            // Create a test markdown file in the source directory
            var testMdFile = Path.Combine(sourceDir, "test.md");
            File.WriteAllText(testMdFile, "---\ntitle: Test Page\n---\n# Test Content");
            
            // Create a dummy file in the output directory that should be cleared
            var dummyFile = Path.Combine(outputDir, "old-file.txt");
            File.WriteAllText(dummyFile, "This should be deleted");
            
            // Create a dummy subdirectory in the output directory
            var dummyDir = Path.Combine(outputDir, "old-directory");
            Directory.CreateDirectory(dummyDir);
            var dummyFileInDir = Path.Combine(dummyDir, "nested-file.txt");
            File.WriteAllText(dummyFileInDir, "This should also be deleted");
            
            try
            {
                // Verify files exist before build
                Assert.True(File.Exists(dummyFile), "Dummy file should exist before build");
                Assert.True(Directory.Exists(dummyDir), "Dummy directory should exist before build");
                
                // Create config
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("root", tempDir),
                        new KeyValuePair<string, string>("out", outputDir),
                        new KeyValuePair<string, string>("normalizePaths", "true")
                    })
                    .Build();
                
                // Act: Run the ClearOutputDirectory method via reflection
                var buildCommand = new FullBuildCommand();
                var method = typeof(FullBuildCommand).GetMethod("ClearOutputDirectory", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(buildCommand, new object[] { config });
                
                // Assert: Verify files were deleted
                Assert.False(File.Exists(dummyFile), "Dummy file should be deleted after clearing");
                Assert.False(Directory.Exists(dummyDir), "Dummy directory should be deleted after clearing");
                Assert.True(Directory.Exists(outputDir), "Output directory itself should still exist");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
        
        [Fact]
        public void ClearOutputDirectory_ShouldHandleNonExistentDirectory()
        {
            // Arrange: Create temp directory that doesn't have an output dir yet
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var outputDir = Path.Combine(tempDir, "out");
            
            try
            {
                // Create config pointing to non-existent output dir
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("out", outputDir)
                    })
                    .Build();
                
                // Act: Run the ClearOutputDirectory method - should not throw
                var buildCommand = new FullBuildCommand();
                var method = typeof(FullBuildCommand).GetMethod("ClearOutputDirectory", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                // Assert: Should complete without throwing an exception
                var exception = Record.Exception(() => method.Invoke(buildCommand, new object[] { config }));
                Assert.Null(exception);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
        
        [Fact]
        public void ClearOutputDirectory_ShouldHandleEmptyOutputPath()
        {
            // Arrange: Create config with empty output path
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("out", "")
                })
                .Build();
            
            // Act: Run the ClearOutputDirectory method - should not throw
            var buildCommand = new FullBuildCommand();
            var method = typeof(FullBuildCommand).GetMethod("ClearOutputDirectory", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Assert: Should complete without throwing an exception
            var exception = Record.Exception(() => method.Invoke(buildCommand, new object[] { config }));
            Assert.Null(exception);
        }

        [Fact]
        public async Task BuildCommand_ShouldWriteRenderedHtmlFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var templatesDir = Path.Combine(tempDir, "templates");
            var outputDir = Path.Combine(tempDir, "out");

            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(templatesDir);
            Directory.CreateDirectory(outputDir);

            try
            {
                await File.WriteAllTextAsync(
                    Path.Combine(sourceDir, "index.md"),
                    "---\ntitle: Home\n---\n# Hello");

                await File.WriteAllTextAsync(
                    Path.Combine(templatesDir, "Default.cshtml"),
                    "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>\n@Raw(Model.Body)");

                await File.WriteAllTextAsync(
                    Path.Combine(templatesDir, "SiteFrame.cshtml"),
                    "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\n<!DOCTYPE html><html><body>@Raw(Model.Body)</body></html>");

                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "root", tempDir },
                        { "out", outputDir },
                        { "templates_folder", "templates" },
                        { "defaultLocale", "en" },
                        { "supportedLocales:0", "en" },
                        { "areas:0:source_folder", "/source" },
                        { "areas:0:output_folder", "/" }
                    })
                    .Build();

                SiteConfig.Instance = config;

                var buildCommand = new FullBuildCommand();
                await buildCommand.RunAsync(config);

                var outputFile = Path.Combine(outputDir, "index.html");
                Assert.True(File.Exists(outputFile));
                Assert.Contains("<h1>Hello</h1>", await File.ReadAllTextAsync(outputFile));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }

        [Fact]
        public async Task BuildCommand_ShouldFailWhenTemplateRenderingFails()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var templatesDir = Path.Combine(tempDir, "templates");
            var outputDir = Path.Combine(tempDir, "out");

            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(templatesDir);
            Directory.CreateDirectory(outputDir);

            try
            {
                await File.WriteAllTextAsync(
                    Path.Combine(sourceDir, "index.md"),
                    "---\ntitle: Home\n---\n# Hello");

                await File.WriteAllTextAsync(
                    Path.Combine(templatesDir, "Default.cshtml"),
                    "@using Missing.Template.Namespace\n@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>\n@Raw(Model.Body)");

                await File.WriteAllTextAsync(
                    Path.Combine(templatesDir, "SiteFrame.cshtml"),
                    "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\n<!DOCTYPE html><html><body>@Raw(Model.Body)</body></html>");

                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "root", tempDir },
                        { "out", outputDir },
                        { "templates_folder", "templates" },
                        { "defaultLocale", "en" },
                        { "supportedLocales:0", "en" },
                        { "areas:0:source_folder", "/source" },
                        { "areas:0:output_folder", "/" }
                    })
                    .Build();

                SiteConfig.Instance = config;

                var buildCommand = new FullBuildCommand();
                var exception = await Assert.ThrowsAsync<ChloroplastException>(() => buildCommand.RunAsync(config));

                Assert.Contains("Build failed", exception.Message);
                Assert.False(File.Exists(Path.Combine(outputDir, "index.html")));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
    }
}
