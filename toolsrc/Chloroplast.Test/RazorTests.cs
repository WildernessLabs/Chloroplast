using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Rendering;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    public class RazorTests
    {
        [Fact]
        public async Task MissingTemplate_ReturnsEmptyStringAndLogsWarning()
        {
            // Arrange
            RazorRenderer renderer = new RazorRenderer();
            string missingTemplateName = "NonExistentTemplate";
            var model = new { TestProperty = "TestValue" };
            
            // Capture console output
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            var result = await renderer.RenderTemplateContent(missingTemplateName, model);

            // Restore console output
            Console.SetOut(originalOut);
            var consoleOutput = stringWriter.ToString();

            // Assert
            Assert.Equal(string.Empty, result.ToString());
            Assert.Contains("Warning: Template 'NonExistentTemplate' not found", consoleOutput);
        }

        [Fact]
        public async Task TemplateResolution_SupportsSubdirectories()
        {
            // Arrange - Create a temporary directory structure with templates
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            var subDir = Path.Combine(templatesDir, "subdirectory");
            
            Directory.CreateDirectory(subDir);
            
            // Create a template in subdirectory
            var templateContent = "@inherits MiniRazor.TemplateBase<string>\n@Model";
            var templatePath = Path.Combine(subDir, "TestTemplate.cshtml");
            await File.WriteAllTextAsync(templatePath, templateContent);
            
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
                // Suppress console output during initialization
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                // Act
                await renderer.InitializeAsync(config);
                
                // Test that template can be found by relative path
                var result = await renderer.RenderTemplateContent("subdirectory/TestTemplate", "Hello World");
                
                Console.SetOut(originalOut);
                
                // Assert
                Assert.Contains("Hello World", result.ToString());
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
        public async Task TemplateResolution_BackwardCompatibility_FilenameOnly()
        {
            // Arrange - Create templates to test backward compatibility
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            // Create a template at root level
            var templateContent = "@inherits MiniRazor.TemplateBase<string>\nRoot: @Model";
            var templatePath = Path.Combine(templatesDir, "MyTemplate.cshtml");
            await File.WriteAllTextAsync(templatePath, templateContent);
            
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
                
                // Test that template can still be found by filename only (backward compatibility)
                var result = await renderer.RenderTemplateContent("MyTemplate", "Test");
                
                Console.SetOut(originalOut);
                
                // Assert
                Assert.Contains("Root: Test", result.ToString());
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
        public async Task TemplateResolution_WithCshtmlExtension()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            var subDir = Path.Combine(templatesDir, "partials");
            
            Directory.CreateDirectory(subDir);
            
            var templateContent = "@inherits MiniRazor.TemplateBase<string>\nPartial: @Model";
            var templatePath = Path.Combine(subDir, "Header.cshtml");
            await File.WriteAllTextAsync(templatePath, templateContent);
            
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
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                await renderer.InitializeAsync(config);
                
                // Test with .cshtml extension
                var result1 = await renderer.RenderTemplateContent("partials/Header.cshtml", "WithExt");
                // Test without .cshtml extension
                var result2 = await renderer.RenderTemplateContent("partials/Header", "WithoutExt");
                
                Console.SetOut(originalOut);
                
                // Both should work
                Assert.Contains("Partial: WithExt", result1.ToString());
                Assert.Contains("Partial: WithoutExt", result2.ToString());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public async Task TemplateResolution_NormalizesPathSeparators()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            var subDir = Path.Combine(templatesDir, "components");
            
            Directory.CreateDirectory(subDir);
            
            var templateContent = "@inherits MiniRazor.TemplateBase<string>\nComponent: @Model";
            var templatePath = Path.Combine(subDir, "Nav.cshtml");
            await File.WriteAllTextAsync(templatePath, templateContent);
            
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
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                await renderer.InitializeAsync(config);
                
                // Test with forward slash
                var result1 = await renderer.RenderTemplateContent("components/Nav", "Forward");
                // Test with backslash (Windows-style)
                var result2 = await renderer.RenderTemplateContent("components\\Nav", "Backward");
                
                Console.SetOut(originalOut);
                
                // Both should work
                Assert.Contains("Component: Forward", result1.ToString());
                Assert.Contains("Component: Backward", result2.ToString());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public async Task TemplateResolution_StripsTemplatesPrefix()
        {
            // Test that "templates/TemplateName" works as fallback by stripping the prefix
            // Arrange - Create a template
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            var templateContent = "@inherits MiniRazor.TemplateBase<string>\nTemplate: @Model";
            var templatePath = Path.Combine(templatesDir, "MyTemplate.cshtml");
            await File.WriteAllTextAsync(templatePath, templateContent);
            
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
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                await renderer.InitializeAsync(config);
                
                // Act - Try to reference with "templates/" prefix (user mistake)
                var result = await renderer.RenderTemplateContent("templates/MyTemplate", "Test Content");
                
                Console.SetOut(originalOut);
                
                // Assert - Should still work by stripping the prefix
                Assert.Contains("Template: Test Content", result.ToString());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public async Task RazorArtifacts_AreGeneratedDuringCompilation()
        {
            // Arrange - Create a temporary directory structure with templates
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            // Create a valid template
            var templateContent = "@inherits MiniRazor.TemplateBase<string>\n<h1>@Model</h1>";
            var templatePath = Path.Combine(templatesDir, "TestTemplate.cshtml");
            await File.WriteAllTextAsync(templatePath, templateContent);
            
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
                // Suppress console output during initialization
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                // Act
                await renderer.InitializeAsync(config);
                
                Console.SetOut(originalOut);
                
                // Assert - Check that artifacts were generated
                var artifactsPath = Path.Combine(tempDir, ".chloroplast", "artifacts", "razor");
                Assert.True(Directory.Exists(artifactsPath), "Artifacts directory should be created");
                
                var artifactFiles = Directory.GetFiles(artifactsPath, "*.cs");
                Assert.NotEmpty(artifactFiles);
                Assert.Contains(artifactFiles, f => f.Contains("TestTemplate"));
                
                // Check that .gitignore was created
                var gitignorePath = Path.Combine(tempDir, ".chloroplast", ".gitignore");
                Assert.True(File.Exists(gitignorePath), ".gitignore should be created");
                Assert.Equal("*\n", File.ReadAllText(gitignorePath));
                
                // Check that README.txt was created
                var readmePath = Path.Combine(tempDir, ".chloroplast", "README.txt");
                Assert.True(File.Exists(readmePath), "README.txt should be created");
                var readmeContent = File.ReadAllText(readmePath);
                Assert.Contains("build artifacts", readmeContent.ToLower());
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
        public async Task RazorCompilationError_IncludesArtifactPath()
        {
            // Arrange - Create a template with a compilation error
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templatesDir = Path.Combine(tempDir, "templates");
            
            Directory.CreateDirectory(templatesDir);
            
            // Create a template with an error (accessing non-existent property)
            var templateContent = "@inherits MiniRazor.TemplateBase<string>\n@Model.NonExistentProperty";
            var templatePath = Path.Combine(templatesDir, "BrokenTemplate.cshtml");
            await File.WriteAllTextAsync(templatePath, templateContent);
            
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
                // Suppress console output during initialization
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                // Act & Assert
                var exception = await Assert.ThrowsAsync<Exception>(async () =>
                {
                    await renderer.InitializeAsync(config);
                });
                
                Console.SetOut(originalOut);
                
                // Verify the exception message includes the artifact path
                Assert.Contains("Failed to compile Razor template", exception.Message);
                Assert.Contains("Generated C# source code saved to:", exception.Message);
                Assert.Contains(".chloroplast/artifacts/razor/BrokenTemplate_", exception.Message);
                
                // Verify the artifact file was actually created
                var artifactsPath = Path.Combine(tempDir, ".chloroplast", "artifacts", "razor");
                var artifactFiles = Directory.GetFiles(artifactsPath, "BrokenTemplate_*.cs");
                Assert.NotEmpty(artifactFiles);
                
                // Verify the generated file contains the problematic code
                var artifactContent = File.ReadAllText(artifactFiles[0]);
                Assert.Contains("NonExistentProperty", artifactContent);
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

        //[Fact]
        public async Task SimpleRender()
        {
            RazorRenderer renderer = new RazorRenderer ();
            
            var content = await renderer.RenderContentAsync (MakeContent ());

            // TODO: need to mock up initialization
        }

        private RenderedContent MakeContent ()
        {
            throw new NotImplementedException ();
        }
    }
}
