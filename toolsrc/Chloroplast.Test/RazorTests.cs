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
