using System;
using System.Linq;
using System.IO;

namespace Chloroplast.Core.Extensions
{
    public static class PathExtensions
    {
        public static char OtherDirectorySeparator { get; } = Path.DirectorySeparatorChar == '/' ? '\\' : '/';

        public static string NormalizePath(this string value, bool toLower = false)
        {
            if (string.IsNullOrWhiteSpace (value))
                return string.Empty;

            var slashed = value.Replace (OtherDirectorySeparator, Path.DirectorySeparatorChar);

            if (slashed.StartsWith ('~'))
                slashed = slashed.Replace("~", Environment.GetFolderPath (Environment.SpecialFolder.UserProfile));

            // Resolve relative paths against the current working directory, and normalize dot-segments
            var fullPath = Path.IsPathRooted(slashed) ? slashed : Path.GetFullPath(slashed);
            try
            {
                // Path.GetFullPath also collapses ../ and ./ segments; if input was rooted it stays as-is
                if (Path.IsPathRooted(slashed))
                    fullPath = Path.GetFullPath(slashed);
            }
            catch { /* if invalid path, keep as-is to fail later in a predictable place */ }

            return toLower ? fullPath.ToLower() : fullPath;
        }

        public static string RelativePath(this string value, string rootPath)
        {
            return RelativePath(value, rootPath, toLower: false);
        }

        public static string RelativePath(this string value, string rootPath, bool toLower)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(rootPath))
                return string.Empty;

            // Normalize both paths to absolute to ensure consistent relative calculation
            var fullValue = value.NormalizePath();
            var fullRoot = rootPath.NormalizePath();

            // Use framework helper to compute a relative path with platform separators
            var rel = Path.GetRelativePath(fullRoot, fullValue);

            // Normalize separators and trim any accidental leading separators
            rel = rel.Replace(OtherDirectorySeparator, Path.DirectorySeparatorChar)
                     .TrimStart(Path.DirectorySeparatorChar);

            return toLower ? rel.ToLower() : rel;
        }

        public static string GetPathFileName(this string value)
        {
            return Path.GetFileName (value);
        }

        // Sanitize a relative path segment: normalize separators, trim leading separators, optional lowercase
        public static string SanitizeRelativeSegment(this string value, bool toLower = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var s = value.Replace(OtherDirectorySeparator, Path.DirectorySeparatorChar)
                         .TrimStart(Path.DirectorySeparatorChar);

            return toLower ? s.ToLower() : s;
        }

        // Normalize a URL path segment for web links (always use forward slashes, optional lowercase)
        public static string NormalizeUrlSegment(this string value, bool toLower = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            // Replace all directory separators with forward slashes for URLs
            var normalized = value.Replace(Path.DirectorySeparatorChar, '/')
                                  .Replace('\\', '/') // Ensure backslashes are converted
                                  .TrimStart('/');

            return toLower ? normalized.ToLower() : normalized;
        }

        public static string CombinePath(this string value, params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return value;

            // Sanitize each segment's separators to the platform default
            var sanitized = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                var seg = paths[i] ?? string.Empty;
                sanitized[i] = seg.Replace(OtherDirectorySeparator, Path.DirectorySeparatorChar);
            }

            // Detect if the first provided path segment started with the "other" separator (e.g., "\\foo")
            string firstOriginal = paths.FirstOrDefault(p => !string.IsNullOrEmpty(p)) ?? string.Empty;
            bool firstStartedWithOtherSep = firstOriginal.Length > 0 && firstOriginal[0] == OtherDirectorySeparator;

            string combinedPaths = Path.Combine(sanitized);

            // If the result appears rooted only because of a leading separator that came from the other separator,
            // treat it as relative by trimming the leading separator.
            if (Path.IsPathRooted(combinedPaths) && firstStartedWithOtherSep)
            {
                combinedPaths = combinedPaths.TrimStart(Path.DirectorySeparatorChar);
            }

            // Combine with the base value. Only normalize to absolute if the result is rooted;
            // otherwise preserve relative semantics for callers that expect relative paths.
            var finalCombined = Path.Combine(value, combinedPaths);
            return Path.IsPathRooted(finalCombined) ? finalCombined.NormalizePath() : finalCombined.Replace(OtherDirectorySeparator, Path.DirectorySeparatorChar);
        }

        /// <returns>The directory path</returns>
        public static string EnsureDirectory(this string dir)
        {
            if (!Directory.Exists (dir))
                Directory.CreateDirectory (dir);

            return dir;
        }

        /// <returns>The directory path</returns>
        public static string EnsureFileDirectory(this string value)
        {
            string dir = Path.GetDirectoryName(value);
            return dir.EnsureDirectory ();
        }
    }
}
