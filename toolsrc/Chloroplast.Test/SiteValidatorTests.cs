using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Chloroplast.Core;

namespace Chloroplast.Test
{
    public class SiteValidatorTests : IDisposable
    {
        private readonly string _testOutputPath;

        public SiteValidatorTests()
        {
            _testOutputPath = Path.Combine(Path.GetTempPath(), "chloroplast_validator_test_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testOutputPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testOutputPath))
            {
                Directory.Delete(_testOutputPath, true);
            }
        }

        [Fact]
        public void Validate_OutputDirectoryNotFound_AddsError()
        {
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid().ToString());
            var validator = new SiteValidator(nonExistentPath);

            validator.Validate();

            Assert.True(validator.HasIssues);
            Assert.Single(validator.Issues);
            Assert.Equal(ValidationSeverity.Error, validator.Issues[0].Severity);
            Assert.Contains("Output directory not found", validator.Issues[0].Category);
        }

        [Fact]
        public void Validate_NoHtmlFiles_AddsWarning()
        {
            var validator = new SiteValidator(_testOutputPath);

            validator.Validate();

            Assert.True(validator.HasIssues);
            Assert.Single(validator.Issues);
            Assert.Equal(ValidationSeverity.Warning, validator.Issues[0].Severity);
            Assert.Contains("No HTML files found", validator.Issues[0].Category);
        }

        [Fact]
        public void Validate_ValidHtmlWithExistingAssets_NoIssues()
        {
            // Create test structure
            var cssDir = Path.Combine(_testOutputPath, "assets");
            Directory.CreateDirectory(cssDir);
            File.WriteAllText(Path.Combine(cssDir, "style.css"), "body { color: black; }");
            File.WriteAllText(Path.Combine(cssDir, "app.js"), "console.log('test');");

            var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <link href=""/assets/style.css"" rel=""stylesheet"" />
</head>
<body>
    <script src=""/assets/app.js""></script>
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.False(validator.HasIssues);
        }

        [Fact]
        public void Validate_MissingCssFile_AddsWarning()
        {
            var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <link href=""/assets/missing.css"" rel=""stylesheet"" />
</head>
<body></body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.True(validator.HasIssues);
            var cssIssue = validator.Issues.FirstOrDefault(i => i.Category.Contains("css"));
            Assert.NotNull(cssIssue);
            Assert.Equal(ValidationSeverity.Warning, cssIssue.Severity);
        }

        [Fact]
        public void Validate_MissingJavaScriptFile_AddsWarning()
        {
            var htmlContent = @"
<!DOCTYPE html>
<html>
<head></head>
<body>
    <script src=""/js/missing.js""></script>
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.True(validator.HasIssues);
            var jsIssue = validator.Issues.FirstOrDefault(i => i.Category.Contains("javascript"));
            Assert.NotNull(jsIssue);
            Assert.Equal(ValidationSeverity.Warning, jsIssue.Severity);
        }

        [Fact]
        public void Validate_BrokenInternalLink_AddsWarning()
        {
            var htmlContent = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""/missing-page.html"">Missing Page</a>
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.True(validator.HasIssues);
            var linkIssue = validator.Issues.FirstOrDefault(i => i.Category.Contains("navigation"));
            Assert.NotNull(linkIssue);
            Assert.Equal(ValidationSeverity.Warning, linkIssue.Severity);
        }

        [Fact]
        public void Validate_ExternalLinks_NoIssues()
        {
            var htmlContent = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""http://example.com"">External</a>
    <a href=""https://example.com"">Secure External</a>
    <a href=""mailto:test@example.com"">Email</a>
    <a href=""#section"">Anchor</a>
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.False(validator.HasIssues);
        }

        [Fact]
        public void Validate_MissingImageFile_AddsWarning()
        {
            var htmlContent = @"
<!DOCTYPE html>
<html>
<body>
    <img src=""/images/missing.png"" alt=""Missing"" />
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.True(validator.HasIssues);
            var imageIssue = validator.Issues.FirstOrDefault(i => i.Category.Contains("image"));
            Assert.NotNull(imageIssue);
            Assert.Equal(ValidationSeverity.Warning, imageIssue.Severity);
        }

        [Fact]
        public void Validate_RelativePathsWithExistingFiles_NoIssues()
        {
            // Create subdirectory with assets
            var subDir = Path.Combine(_testOutputPath, "subdir");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "style.css"), "body {}");

            var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <link href=""style.css"" rel=""stylesheet"" />
</head>
</html>";
            File.WriteAllText(Path.Combine(subDir, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.False(validator.HasIssues);
        }

        [Fact]
        public void Validate_UrlsWithQueryStringsAndFragments_ValidatedCorrectly()
        {
            // Create assets
            var cssDir = Path.Combine(_testOutputPath, "assets");
            Directory.CreateDirectory(cssDir);
            File.WriteAllText(Path.Combine(cssDir, "style.css"), "body {}");

            var htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <link href=""/assets/style.css?v=123"" rel=""stylesheet"" />
</head>
<body>
    <a href=""page.html#section"">Section</a>
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);
            File.WriteAllText(Path.Combine(_testOutputPath, "page.html"), "<html></html>");

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.False(validator.HasIssues);
        }

        [Fact]
        public void Validate_DirectoryLinkWithIndexHtml_NoIssues()
        {
            // Create subdirectory with index.html
            var subDir = Path.Combine(_testOutputPath, "docs");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "index.html"), "<html></html>");

            var htmlContent = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""/docs/"">Docs</a>
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.False(validator.HasIssues);
        }

        [Fact]
        public void Validate_DirectoryLinkWithoutIndexHtml_AddsWarning()
        {
            // Create subdirectory without index.html
            var subDir = Path.Combine(_testOutputPath, "docs");
            Directory.CreateDirectory(subDir);

            var htmlContent = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""/docs/"">Docs</a>
</body>
</html>";
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), htmlContent);

            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            Assert.True(validator.HasIssues);
            var indexIssue = validator.Issues.FirstOrDefault(i => i.Category.Contains("index"));
            Assert.NotNull(indexIssue);
        }

        [Fact]
        public void WriteIssuesToConsole_NoIssues_WritesSuccessMessage()
        {
            File.WriteAllText(Path.Combine(_testOutputPath, "index.html"), "<html></html>");
            
            var validator = new SiteValidator(_testOutputPath);
            validator.Validate();

            // This test verifies the method runs without exception
            // Actual console output testing would require capturing Console.Out
            validator.WriteIssuesToConsole();
            
            Assert.False(validator.HasIssues);
        }
    }
}
