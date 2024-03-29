using System;
using System.IO;
using Chloroplast.Core.Extensions;
using Xunit;

namespace Chloroplast.Test
{
    public class PathTests
    {
        [Fact]
        public void NormalizesDirCharToPlatform ()
        {
            var mixedSlashPath = $"this{PathExtensions.OtherDirectorySeparator}path{Path.DirectorySeparatorChar}yes";
            Assert.Equal ($"this{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}yes", mixedSlashPath.NormalizePath ());
        }

        [Fact]
        public void ExpandsHomeDirectory ()
        {
            var homePath = Path.Combine("~", "dev");
            var actual = homePath.NormalizePath ();

            Assert.False (actual.Contains ('~'), $"didn't expand home path in {actual}");
            Assert.True (actual.Length > 5, $"shorter than expected {actual}");
        }

        [Fact]
        public void GetsRelativePath ()
        {
            var rootPath = Path.Combine ("c:", "home", "dir", "site");
            var fullPath = Path.Combine (rootPath, "area", "index.md");
            var actual = fullPath.RelativePath (rootPath);
            Assert.DoesNotContain (rootPath, actual);
            Assert.False (actual.StartsWith (Path.DirectorySeparatorChar), $"Should not start with slash char, {actual}");
        }

        [Fact]
        public void JoinsPaths ()
        {
            var joined = "some".CombinePath ("\\path");
            Assert.Equal (Path.Combine ("some", "path"), joined);
        }
    }
}
