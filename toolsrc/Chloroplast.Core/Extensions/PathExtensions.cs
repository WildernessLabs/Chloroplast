using System;
using System.IO;
using System.Linq;

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

            return toLower ? slashed.ToLower() : slashed;
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
            if (value == null) value = string.Empty;

            string combinedPaths = Path.Combine (paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray()).NormalizePath ();
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
