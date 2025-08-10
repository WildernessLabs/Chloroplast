using System;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Core
{
    public static class SiteConfig
    {
        public static IConfigurationRoot Instance { get; set; }
        public static string BuildVersion { get; set; }
        public static bool CacheBustingEnabled => Instance?.GetBool("cacheBusting:enabled", defaultValue: true) ?? true;

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
                var raw = Instance?["basePath"]; // may be null
                if (string.IsNullOrWhiteSpace(raw))
                    return string.Empty;

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
    }
}
