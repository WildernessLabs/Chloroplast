using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chloroplast.Core
{
    public class SiteValidator
    {
        private readonly string _outputPath;
        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();

        public SiteValidator(string outputPath)
        {
            _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        }

        public IReadOnlyList<ValidationIssue> Issues => _issues.AsReadOnly();
        public bool HasIssues => _issues.Any();

        public void Validate()
        {
            _issues.Clear();

            if (!Directory.Exists(_outputPath))
            {
                _issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Output directory not found",
                    $"The output directory does not exist: {_outputPath}",
                    null));
                return;
            }

            // Get all HTML files
            var htmlFiles = Directory.GetFiles(_outputPath, "*.html", SearchOption.AllDirectories);
            
            if (!htmlFiles.Any())
            {
                _issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "No HTML files found",
                    $"No HTML files were found in the output directory: {_outputPath}",
                    null));
                return;
            }

            Console.WriteLine($"Validating {htmlFiles.Length} HTML file(s)...");

            // Validate each HTML file
            foreach (var htmlFile in htmlFiles)
            {
                ValidateHtmlFile(htmlFile);
            }
        }

        private void ValidateHtmlFile(string htmlFilePath)
        {
            try
            {
                var content = File.ReadAllText(htmlFilePath);
                var relativePath = Path.GetRelativePath(_outputPath, htmlFilePath);

                // Check for broken links to CSS files
                var cssLinks = Regex.Matches(content, @"<link[^>]+href=[""']([^""']+\.css[^""']*)[""']", RegexOptions.IgnoreCase);
                foreach (Match match in cssLinks)
                {
                    ValidateAssetLink(match.Groups[1].Value, relativePath, htmlFilePath, "CSS");
                }

                // Check for broken links to JavaScript files
                var scriptLinks = Regex.Matches(content, @"<script[^>]+src=[""']([^""']+\.js[^""']*)[""']", RegexOptions.IgnoreCase);
                foreach (Match match in scriptLinks)
                {
                    ValidateAssetLink(match.Groups[1].Value, relativePath, htmlFilePath, "JavaScript");
                }

                // Check for broken navigation/internal links (href to .html or paths without extensions)
                var navLinks = Regex.Matches(content, @"<a[^>]+href=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                foreach (Match match in navLinks)
                {
                    var href = match.Groups[1].Value;
                    // Skip external links, anchors, and special protocols
                    if (href.StartsWith("http://") || href.StartsWith("https://") || 
                        href.StartsWith("#") || href.StartsWith("mailto:") || 
                        href.StartsWith("javascript:") || href.StartsWith("tel:"))
                        continue;

                    ValidateNavigationLink(href, relativePath, htmlFilePath);
                }

                // Check for broken image links
                var imageLinks = Regex.Matches(content, @"<img[^>]+src=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                foreach (Match match in imageLinks)
                {
                    ValidateAssetLink(match.Groups[1].Value, relativePath, htmlFilePath, "Image");
                }
            }
            catch (Exception ex)
            {
                _issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "File read error",
                    $"Error reading file: {Path.GetRelativePath(_outputPath, htmlFilePath)}",
                    ex.Message));
            }
        }

        private void ValidateAssetLink(string assetUrl, string relativePath, string htmlFilePath, string assetType)
        {
            // Remove query string and fragment
            var cleanUrl = assetUrl.Split('?', '#')[0];
            
            // Skip data URLs and external URLs
            if (cleanUrl.StartsWith("data:") || cleanUrl.StartsWith("http://") || cleanUrl.StartsWith("https://") || cleanUrl.StartsWith("//"))
                return;

            string assetPath;
            if (cleanUrl.StartsWith("/"))
            {
                // Absolute path from site root
                assetPath = Path.Combine(_outputPath, cleanUrl.TrimStart('/'));
            }
            else
            {
                // Relative path from the HTML file
                var htmlDir = Path.GetDirectoryName(htmlFilePath);
                assetPath = Path.Combine(htmlDir, cleanUrl);
            }

            // Normalize path
            assetPath = Path.GetFullPath(assetPath);

            if (!File.Exists(assetPath))
            {
                _issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    $"Missing {assetType.ToLower()} file",
                    $"In '{relativePath}': {assetType} file not found: {cleanUrl}",
                    null));
            }
        }

        private void ValidateNavigationLink(string href, string relativePath, string htmlFilePath)
        {
            // Remove query string and fragment for file existence check
            var cleanUrl = href.Split('?', '#')[0];
            
            // Skip empty hrefs
            if (string.IsNullOrWhiteSpace(cleanUrl))
                return;

            string targetPath;
            if (cleanUrl.StartsWith("/"))
            {
                // Absolute path from site root
                targetPath = Path.Combine(_outputPath, cleanUrl.TrimStart('/'));
            }
            else
            {
                // Relative path from the HTML file
                var htmlDir = Path.GetDirectoryName(htmlFilePath);
                targetPath = Path.Combine(htmlDir, cleanUrl);
            }

            // Normalize path
            targetPath = Path.GetFullPath(targetPath);

            // If it's a directory path, check for index.html
            if (Directory.Exists(targetPath))
            {
                var indexPath = Path.Combine(targetPath, "index.html");
                if (!File.Exists(indexPath))
                {
                    _issues.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "Missing index file",
                        $"In '{relativePath}': Directory link has no index.html: {cleanUrl}",
                        null));
                }
            }
            else if (!File.Exists(targetPath))
            {
                // Check if it's a file that should exist
                var ext = Path.GetExtension(cleanUrl);
                if (string.IsNullOrEmpty(ext) || ext == ".html" || ext == ".htm")
                {
                    _issues.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "Broken navigation link",
                        $"In '{relativePath}': Navigation link target not found: {cleanUrl}",
                        null));
                }
            }
        }

        public void WriteIssuesToConsole()
        {
            if (!_issues.Any())
            {
                Console.WriteLine("✓ Validation passed - no issues found");
                return;
            }

            var errorCount = _issues.Count(i => i.Severity == ValidationSeverity.Error);
            var warningCount = _issues.Count(i => i.Severity == ValidationSeverity.Warning);

            Console.WriteLine();
            Console.WriteLine("=== VALIDATION ISSUES ===");
            Console.WriteLine($"Found {errorCount} error(s) and {warningCount} warning(s)");
            Console.WriteLine();

            // Group issues by severity
            var errors = _issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
            var warnings = _issues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();

            if (errors.Any())
            {
                Console.WriteLine("ERRORS:");
                foreach (var issue in errors)
                {
                    Console.WriteLine($"  [{issue.Category}] {issue.Message}");
                    if (!string.IsNullOrEmpty(issue.Details))
                    {
                        Console.WriteLine($"    Details: {issue.Details}");
                    }
                }
                Console.WriteLine();
            }

            if (warnings.Any())
            {
                Console.WriteLine("WARNINGS:");
                foreach (var issue in warnings)
                {
                    Console.WriteLine($"  [{issue.Category}] {issue.Message}");
                }
            }
        }
    }

    public enum ValidationSeverity
    {
        Warning,
        Error
    }

    public class ValidationIssue
    {
        public ValidationSeverity Severity { get; }
        public string Category { get; }
        public string Message { get; }
        public string Details { get; }

        public ValidationIssue(ValidationSeverity severity, string category, string message, string details)
        {
            Severity = severity;
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Details = details;
        }
    }
}
