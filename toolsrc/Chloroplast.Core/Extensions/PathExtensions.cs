using System;
using System.IO;

namespace Chloroplast.Core.Extensions
{
    public static class PathExtensions
    {
        public static char OtherDirectorySeparator { get; } = Path.DirectorySeparatorChar == '/' ? '\\' : '/';

        public static string NormalizePath(this string value)
        {
            if (string.IsNullOrWhiteSpace (value))
                return string.Empty;

            var slashed = value.Replace (OtherDirectorySeparator, Path.DirectorySeparatorChar);

            if (slashed.StartsWith ('~'))
                slashed = slashed.Replace("~", Environment.GetFolderPath (Environment.SpecialFolder.UserProfile));

            return slashed;
        }

        public static string RelativePath(this string value, string rootPath)
        {
            var replaced = value.Normalize ().Replace (rootPath.Normalize (), string.Empty);
            if (replaced.StartsWith (Path.DirectorySeparatorChar))
                replaced = replaced.Substring (1);

            return replaced;
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

        public static void EnsureFileDirectory(this string value)
        {
            string dir = Path.GetDirectoryName(value);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
