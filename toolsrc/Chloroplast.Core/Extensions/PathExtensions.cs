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

            // Ensure relative paths are resolved against the current working directory
            var fullPath = Path.IsPathRooted(slashed) ? slashed : Path.GetFullPath(slashed);

            return toLower ? fullPath.ToLower() : fullPath;
        }

        public static string RelativePath(this string value, string rootPath)
        {
            var replaced = value.Normalize ().Replace (rootPath.Normalize (), string.Empty);
            if (replaced.StartsWith (Path.DirectorySeparatorChar))
                replaced = replaced.Substring (1);

            return replaced;
        }

        public static string GetPathFileName(this string value)
        {
            return Path.GetFileName (value);
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

            // Combine with the base value and return as-is to preserve relative/absolute semantics.
            var finalCombined = Path.Combine(value, combinedPaths);
            return finalCombined;
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
