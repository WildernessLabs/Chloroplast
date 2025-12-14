using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Content;
using Chloroplast.Core.Rendering;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Chloroplast.Test
{
    /// <summary>
    /// Integration tests that demonstrate the feature request:
    /// Partials can access parent page metadata (e.g., for nav active state).
    /// </summary>
    public class PartialMetadataIntegrationTests
    {
        [Fact]
        public async Task NavPartial_CanAccessParentActiveNavMetadata()
        {
            // This test demonstrates the exact scenario from the issue:
            // A nav partial needs to access the parent page's 'activeNav' metadata
            // to determine which navigation item should be marked as active.

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
                // Create a content page with activeNav metadata
                var homePage = @"---
title: Home Page
activeNav: home
---

# Welcome Home

This is the home page.";
                var homePagePath = Path.Combine(sourceDir, "index.md");
                await File.WriteAllTextAsync(homePagePath, homePage);

                // Create another page with different activeNav
                var aboutPage = @"---
title: About Page
activeNav: about
---

# About Us

This is the about page.";
                var aboutPagePath = Path.Combine(sourceDir, "about.md");
                await File.WriteAllTextAsync(aboutPagePath, aboutPage);

                // Create a nav partial that accesses parent's activeNav metadata
                var navPartial = @"---
template: nav
navItems:
  - title: Home
    path: /
    key: home
  - title: About
    path: /about
    key: about
---

@* This partial accesses the parent page's activeNav to highlight the current item *@";
                var navPartialPath = Path.Combine(sourceDir, "nav.md");
                await File.WriteAllTextAsync(navPartialPath, navPartial);

                // Create a nav template that checks for activeNav from parent
                var navTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>\n" +
                    "<nav><div>Active: @Model.GetMeta(\"activeNav\")</div></nav>";
                var navTemplatePath = Path.Combine(templatesDir, "nav.cshtml");
                await File.WriteAllTextAsync(navTemplatePath, navTemplate);

                // Create a Default template that includes the nav partial
                var defaultTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>\n" +
                    "<div class=\"page\">\n" +
                    "    @await PartialAsync(\"source/nav.md\")\n" +
                    "    <main>\n" +
                    "        <h1>@Model.GetMeta(\"title\")</h1>\n" +
                    "        @Raw(Model.Body)\n" +
                    "    </main>\n" +
                    "</div>";
                var defaultTemplatePath = Path.Combine(templatesDir, "Default.cshtml");
                await File.WriteAllTextAsync(defaultTemplatePath, defaultTemplate);

                // Create a minimal SiteFrame template
                var frameTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\n" +
                    "<!DOCTYPE html>\n" +
                    "<html>\n" +
                    "<head><title>@Model.GetMeta(\"title\")</title></head>\n" +
                    "<body>\n" +
                    "@Raw(Model.Body)\n" +
                    "</body>\n" +
                    "</html>";
                var frameTemplatePath = Path.Combine(templatesDir, "SiteFrame.cshtml");
                await File.WriteAllTextAsync(frameTemplatePath, frameTemplate);

                // Create configuration
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "root", tempDir },
                        { "templates_folder", "templates" },
                        { "title", "Test Site" }
                    })
                    .Build();

                // Set SiteConfig.Instance for use in templates
                SiteConfig.Instance = config;

                // Initialize renderer
                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                await ContentRenderer.InitializeAsync(config);

                // Create ContentNode for home page
                var contentArea = new GroupContentArea
                {
                    SourcePath = sourceDir,
                    TargetPath = outputDir
                };

                var homeNode = new ContentNode
                {
                    Slug = "index",
                    Source = new DiskFile(homePagePath, "index.md"),
                    Target = new DiskFile(Path.Combine(outputDir, "index.html"), "index.html"),
                    Area = contentArea,
                    Locale = "en",
                    MenuPath = homePagePath
                };

                // Act - Render the home page
                var homeRendered = await ContentRenderer.FromMarkdownAsync(homeNode);
                homeRendered = await ContentRenderer.ToRazorAsync(homeRendered);

                Console.SetOut(originalOut);

                // Assert - The rendered home page should contain nav with 'home' marked as active
                Assert.NotNull(homeRendered.Body);
                // Debug: print the actual body
                Console.WriteLine("=== Rendered Body ===");
                Console.WriteLine(homeRendered.Body);
                Console.WriteLine("=== End Body ===");
                Assert.Contains("Active: home", homeRendered.Body); // Nav should show activeNav from parent
                
                // The key assertion: verify that the nav partial was able to read the parent's activeNav
                // This is proven by the presence of the active value in the output

                // Now test with About page
                Console.SetOut(stringWriter);
                
                var aboutNode = new ContentNode
                {
                    Slug = "about",
                    Source = new DiskFile(aboutPagePath, "about.md"),
                    Target = new DiskFile(Path.Combine(outputDir, "about.html"), "about.html"),
                    Area = contentArea,
                    Locale = "en",
                    MenuPath = aboutPagePath
                };

                var aboutRendered = await ContentRenderer.FromMarkdownAsync(aboutNode);
                aboutRendered = await ContentRenderer.ToRazorAsync(aboutRendered);

                Console.SetOut(originalOut);

                // Verify About page has About nav item active
                Assert.Contains("Active: about", aboutRendered.Body); // About should be active
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
        public async Task NestedPartials_InheritMetadataChain()
        {
            // Test that when a partial renders another partial,
            // the nested partial has access to the full metadata chain

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sourceDir = Path.Combine(tempDir, "source");
            var templatesDir = Path.Combine(tempDir, "templates");
            var outputDir = Path.Combine(tempDir, "output");
            
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(templatesDir);
            Directory.CreateDirectory(outputDir);

            try
            {
                // Create main content with metadata
                var mainContent = @"---
title: Main Content
pageLevel: mainValue
sharedKey: mainShared
---

# Main Content";
                var mainContentPath = Path.Combine(sourceDir, "main.md");
                await File.WriteAllTextAsync(mainContentPath, mainContent);

                // Create first-level partial with its own metadata
                var firstPartial = @"---
template: first
firstLevel: firstValue
sharedKey: firstShared
---

First partial content";
                var firstPartialPath = Path.Combine(sourceDir, "first.md");
                await File.WriteAllTextAsync(firstPartialPath, firstPartial);

                // Create second-level partial with its own metadata
                var secondPartial = @"---
template: second
secondLevel: secondValue
sharedKey: secondShared
---

Second partial content";
                var secondPartialPath = Path.Combine(sourceDir, "second.md");
                await File.WriteAllTextAsync(secondPartialPath, secondPartial);

                // Create templates that access metadata at each level
                var secondTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>\n" +
                    "<div class=\"second\">\n" +
                    "Page: @Model.GetMeta(\"pageLevel\")\n" +
                    "First: @Model.GetMeta(\"firstLevel\")\n" +
                    "Second: @Model.GetMeta(\"secondLevel\")\n" +
                    "Shared: @Model.GetMeta(\"sharedKey\")\n" +
                    "</div>";
                await File.WriteAllTextAsync(Path.Combine(templatesDir, "second.cshtml"), secondTemplate);

                var firstTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>\n" +
                    "<div class=\"first\">\n" +
                    "@await PartialAsync(\"source/second.md\")\n" +
                    "</div>";
                await File.WriteAllTextAsync(Path.Combine(templatesDir, "first.cshtml"), firstTemplate);

                var defaultTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>\n" +
                    "<div class=\"main\">\n" +
                    "@await PartialAsync(\"source/first.md\")\n" +
                    "</div>";
                await File.WriteAllTextAsync(Path.Combine(templatesDir, "Default.cshtml"), defaultTemplate);

                var frameTemplate = "@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.FrameRenderedContent>\n" +
                    "@Raw(Model.Body)";
                await File.WriteAllTextAsync(Path.Combine(templatesDir, "SiteFrame.cshtml"), frameTemplate);

                // Initialize
                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "root", tempDir },
                        { "templates_folder", "templates" }
                    })
                    .Build();

                // Set SiteConfig.Instance
                SiteConfig.Instance = config;

                var originalOut = Console.Out;
                using var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                await ContentRenderer.InitializeAsync(config);

                var contentArea = new GroupContentArea
                {
                    SourcePath = sourceDir,
                    TargetPath = outputDir
                };

                var mainNode = new ContentNode
                {
                    Slug = "main",
                    Source = new DiskFile(mainContentPath, "main.md"),
                    Target = new DiskFile(Path.Combine(outputDir, "main.html"), "main.html"),
                    Area = contentArea,
                    Locale = "en",
                    MenuPath = mainContentPath
                };

                // Act
                var rendered = await ContentRenderer.FromMarkdownAsync(mainNode);
                rendered = await ContentRenderer.ToRazorAsync(rendered);

                Console.SetOut(originalOut);

                // Assert - The nested partial should have access to all levels
                Assert.Contains("Page: mainValue", rendered.Body); // From main content
                Assert.Contains("First: firstValue", rendered.Body); // From first partial
                Assert.Contains("Second: secondValue", rendered.Body); // From second partial
                // Most importantly: child overrides parent for shared key
                Assert.Contains("Shared: secondShared", rendered.Body); // Second partial overrides
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
