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
    public class FrameSelectionTests : IDisposable
    {
        // Template content constants
        private const string SiteFrameTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\nSiteFrame: @Model.Body";
        private const string CustomFrameTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\nCustomFrame: @Model.Body";

        private readonly string _tempDir;
        private readonly string _templatesDir;
        
        public FrameSelectionTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _templatesDir = Path.Combine(_tempDir, "templates");
            Directory.CreateDirectory(_templatesDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        private async Task<RazorRenderer> CreateRendererWithTemplates(params (string name, string content)[] templates)
        {
            foreach (var (name, content) in templates)
            {
                var templatePath = Path.Combine(_templatesDir, $"{name}.cshtml");
                await File.WriteAllTextAsync(templatePath, content);
            }

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "root", _tempDir },
                    { "templates_folder", "templates" }
                })
                .Build();

            var renderer = new RazorRenderer();
            await renderer.InitializeAsync(config);
            return renderer;
        }

        private T SuppressConsoleOutput<T>(Func<T> action)
        {
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            try
            {
                Console.SetOut(stringWriter);
                return action();
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        private async Task<T> SuppressConsoleOutputAsync<T>(Func<Task<T>> action)
        {
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            try
            {
                Console.SetOut(stringWriter);
                return await action();
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        private (string output, T result) CaptureConsoleOutput<T>(Func<T> action)
        {
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            try
            {
                Console.SetOut(stringWriter);
                var result = action();
                return (stringWriter.ToString(), result);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        private async Task<(string output, T result)> CaptureConsoleOutputAsync<T>(Func<Task<T>> action)
        {
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            try
            {
                Console.SetOut(stringWriter);
                var result = await action();
                return (stringWriter.ToString(), result);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public async Task DefaultFrameIsUsed_WhenNoFrameSpecified()
        {
            // Arrange
            var renderer = await SuppressConsoleOutputAsync(async () => 
                await CreateRendererWithTemplates(("SiteFrame", SiteFrameTemplate)));

            var content = new FrameRenderedContent(
                new RenderedContent 
                { 
                    Body = "<p>Test content</p>",
                    Metadata = new ConfigurationBuilder().Build(),
                    Node = new ContentNode { Title = "Test" }
                },
                new List<ContentNode>()
            );

            // Act
            var result = await SuppressConsoleOutputAsync(async () => 
                await renderer.RenderContentAsync(content));

            // Assert
            Assert.NotNull(result);
            Assert.Contains("SiteFrame:", result);
            Assert.Contains("Test content", result);
        }

        [Fact]
        public async Task CustomFrameIsUsed_WhenFrameSpecified()
        {
            // Arrange
            var renderer = await SuppressConsoleOutputAsync(async () => 
                await CreateRendererWithTemplates(
                    ("SiteFrame", SiteFrameTemplate),
                    ("CustomFrame", CustomFrameTemplate)));

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

            // Act
            var result = await SuppressConsoleOutputAsync(async () => 
                await renderer.RenderContentAsync(content));

            // Assert
            Assert.NotNull(result);
            Assert.Contains("CustomFrame:", result);
            Assert.Contains("Test content with custom frame", result);
            Assert.DoesNotContain("SiteFrame:", result);
        }

        [Fact]
        public async Task MissingFrame_ReturnsNull_AndLogsError()
        {
            // Arrange
            var renderer = await SuppressConsoleOutputAsync(async () => 
                await CreateRendererWithTemplates(("SiteFrame", SiteFrameTemplate)));

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

            // Act
            var (consoleOutput, result) = await CaptureConsoleOutputAsync(async () => 
                await renderer.RenderContentAsync(content));

            // Assert
            Assert.Null(result);
            Assert.Contains("ERROR", consoleOutput);
            Assert.Contains("NonExistentFrame", consoleOutput);
            Assert.Contains("not found", consoleOutput);
        }

        [Fact]
        public async Task MissingSiteFrame_ReturnsNull_AndLogsError()
        {
            // Arrange - Create renderer without SiteFrame
            var renderer = await SuppressConsoleOutputAsync(async () => 
                await CreateRendererWithTemplates()); // No templates

            var content = new FrameRenderedContent(
                new RenderedContent 
                { 
                    Body = "<p>Test content</p>",
                    Metadata = new ConfigurationBuilder().Build(),
                    Node = new ContentNode { Title = "Test Page" }
                },
                new List<ContentNode>()
            );

            // Act
            var (consoleOutput, result) = await CaptureConsoleOutputAsync(async () => 
                await renderer.RenderContentAsync(content));

            // Assert
            Assert.Null(result);
            Assert.Contains("ERROR", consoleOutput);
            Assert.Contains("SiteFrame", consoleOutput);
            Assert.Contains("not found", consoleOutput);
        }
    }
}
