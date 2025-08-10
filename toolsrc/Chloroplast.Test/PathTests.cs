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
            var expected = Path.GetFullPath($"this{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}yes");
            Assert.Equal (expected, mixedSlashPath.NormalizePath ());
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

        [Fact]
        public void CombinePath_WithAbsoluteSecondPath_PreservesAbsolute()
        {
            var root = Path.GetTempPath();
            var abs = Path.Combine(root, "sub", "file.txt");
            var joined = "some".CombinePath(abs);
            Assert.Equal(abs.NormalizePath(), joined.NormalizePath());
        }

        [Fact]
        public void CombinePath_LeadingOtherSeparator_TreatedAsRelative()
        {
            var joined = "/home".CombinePath($"{PathExtensions.OtherDirectorySeparator}user", "docs");
            Assert.Equal(Path.Combine("/home", "user", "docs").NormalizePath(), joined.NormalizePath());
        }

        [Fact]
        public void RelativePath_ToLower_And_NoLeadingSlash()
        {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var full = Path.Combine(root, "Area", "Sub", "FILE.MD");
            var rel = full.RelativePath(root, toLower: true);
            Assert.False(rel.StartsWith(Path.DirectorySeparatorChar));
            Assert.Equal(Path.Combine("area", "sub", "file.md"), rel);
        }

        [Fact]
        public void SanitizeRelativeSegment_TrimsAndNormalizes()
        {
            var mixed = $"{Path.DirectorySeparatorChar}Area{PathExtensions.OtherDirectorySeparator}Index.md";
            var sanitized = mixed.SanitizeRelativeSegment();
            Assert.False(sanitized.StartsWith(Path.DirectorySeparatorChar));
            Assert.Equal(Path.Combine("Area", "Index.md"), sanitized);

            var lower = mixed.SanitizeRelativeSegment(toLower: true);
            Assert.Equal(Path.Combine("area", "index.md"), lower);
        }
    }
}
