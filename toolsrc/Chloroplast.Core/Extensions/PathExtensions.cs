using System;
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
            string combinedPaths = Path.Combine (paths).NormalizePath ();
            if (combinedPaths.StartsWith (Path.DirectorySeparatorChar))
                combinedPaths = combinedPaths.Substring (1);

            if (combinedPaths.StartsWith (Path.DirectorySeparatorChar))
                combinedPaths = combinedPaths.Substring (1);

            return Path.Combine (value, combinedPaths);
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
