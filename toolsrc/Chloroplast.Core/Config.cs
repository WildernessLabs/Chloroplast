using System;
using System.Linq;
using Chloroplast.Core.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Core
{
    public static class SiteConfig
    {
        public static IConfigurationRoot Instance { get; set; }
        public static string BuildVersion { get; set; }
        public static bool CacheBustingEnabled => Instance?.GetBool("cacheBusting:enabled", defaultValue: true) ?? true;
    /// <summary>
    /// When set (via CLI --no-basepath) the effective BasePath is forced to empty for the lifetime
    /// of the process. This is used for local testing so that locally hosted builds don't need to
    /// emulate a production sub-path (e.g., GitHub Pages repository name).
    /// </summary>
    public static bool DisableBasePath { get; set; }
        
        /// <summary>
        /// Gets the default locale for the site content. Defaults to "en" if not specified.
        /// </summary>
        public static string DefaultLocale => Instance?["defaultLocale"] ?? "en";
        
        /// <summary>
        /// Gets the array of supported locales from configuration.
        /// </summary>
        public static string[] SupportedLocales
        {
            get
            {
                var section = Instance?.GetSection("supportedLocales");
                if (section == null || !section.Exists())
                    return new[] { DefaultLocale };
                    
                var locales = section.GetChildren().Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                return locales.Length > 0 ? locales : new[] { DefaultLocale };
            }
        }
    static bool basePathConflictWarned = false;

        /// <summary>
        /// Normalized base path for hosting under a sub-path (e.g., GitHub Pages).
        /// Examples:
        ///  - null/empty => ""
        ///  - "blah" => "/blah"
        ///  - "blah/" => "/blah"
        ///  - "/blah" => "/blah"
        ///  - "/" => ""
        /// </summary>
        public static string BasePath
        {
            get
            {
                if (DisableBasePath)
                    return string.Empty;
                var raw = Instance?["basePath"]; // may be null
                var baseUrl = Instance?["baseUrl"]; // sitemap/base url

                // If both are present, warn once and prefer explicit basePath value
                if (!string.IsNullOrWhiteSpace(raw) && !string.IsNullOrWhiteSpace(baseUrl) && !basePathConflictWarned)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Warning: Both 'basePath' and 'baseUrl' are set. Using 'basePath' and ignoring any path component from 'baseUrl'.");
                        Console.ResetColor();
                    }
                    catch (PlatformNotSupportedException) { /* ignore console color issues in some hosts */ }
                    catch (System.IO.IOException) { /* ignore console color issues in some hosts */ }
                    catch { /* ignore any other console issues */ }
                    finally
                    {
                        basePathConflictWarned = true;
                    }
                }

                if (string.IsNullOrWhiteSpace(raw))
                {
                    // derive from baseUrl if present
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
                        {
                            var path = uri.AbsolutePath ?? string.Empty; // starts with '/'
                            if (string.IsNullOrWhiteSpace(path) || path == "/")
                                return string.Empty;

                            // Trim trailing slash, but preserve leading
                            if (path.Length > 1 && path.EndsWith("/"))
                                path = path.TrimEnd('/');

                            return path; // already starts with '/'
                        }
                        else
                        {
                            // If baseUrl is not absolute, try to parse path-like content conservatively
                            var tentative = baseUrl.Trim();
                            if (!tentative.StartsWith("/")) tentative = "/" + tentative;
                            if (tentative.Length > 1 && tentative.EndsWith("/")) tentative = tentative.TrimEnd('/');
                            return tentative == "/" ? string.Empty : tentative;
                        }
                    }
                    return string.Empty;
                }

                raw = raw.Trim();

                // Ensure leading slash
                if (!raw.StartsWith("/"))
                    raw = "/" + raw;

                // Trim trailing slash (except root-only which we treat as empty)
                if (raw.Length > 1 && raw.EndsWith("/"))
                    raw = raw.TrimEnd('/');

                // Treat "/" as no base path
                if (raw == "/")
                    return string.Empty;

                return raw;
            }
        }

        /// <summary>
        /// Applies BasePath to a site-relative URL path. If the input is an absolute URL (http/https),
        /// it's returned unchanged. Ensures no double slashes are introduced.
        /// </summary>
        /// <param name="path">A site-relative path, e.g., "/assets/main.css" or "assets/main.css" or "/".</param>
        /// <returns>Path prefixed with BasePath, beginning with "/".</returns>
        public static string ApplyBasePath(string path)
        {
            if (DisableBasePath) return path;
            
            if (string.IsNullOrWhiteSpace(path))
                return BasePath.Length == 0 ? "/" : BasePath;

            // Absolute URL? leave it
            if (path.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                return path;

            // Fragment-only? leave it
            if (path.StartsWith("#"))
                return path;

            // Ensure leading slash on the provided path
            var normalizedPath = path.StartsWith("/") ? path : "/" + path;

            if (string.IsNullOrEmpty(BasePath))
                return normalizedPath;

            // BasePath does not end with '/', normalizedPath starts with '/'
            return BasePath + normalizedPath;
        }
        
        /// <summary>
        /// Applies locale prefix and BasePath to a site-relative URL path for localized content.
        /// Default locale content uses the standard path without locale prefix.
        /// </summary>
        /// <param name="path">A site-relative path, e.g., "/docs/guide" or "docs/guide".</param>
        /// <param name="locale">The locale code, e.g., "en", "es", "fr".</param>
        /// <returns>Localized path with BasePath applied.</returns>
        public static string ApplyLocalePath(string path, string locale)
        {
            if (string.IsNullOrWhiteSpace(locale) || locale == DefaultLocale)
                return ApplyBasePath(path);
                
            if (string.IsNullOrWhiteSpace(path))
                return ApplyBasePath($"/{locale}");

            // Absolute URL? leave it
            if (path.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                return path;

            // Fragment-only? leave it
            if (path.StartsWith("#"))
                return path;

            // Ensure leading slash on the provided path
            var normalizedPath = path.StartsWith("/") ? path : "/" + path;
            
            // Apply locale prefix
            var localizedPath = $"/{locale}{normalizedPath}";
            
            if (string.IsNullOrEmpty(BasePath))
                return localizedPath;

            // BasePath does not end with '/', localizedPath starts with '/'
            return BasePath + localizedPath;
        }
    }
}
