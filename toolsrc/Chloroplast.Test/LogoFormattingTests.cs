using System;
using System.Linq;
using Chloroplast.Tool;
using Xunit;

namespace Chloroplast.Test
{
    public class LogoFormattingTests
    {
        [Fact]
        public void GetFormattedLogo_WithDefaultVersion_ReturnsValidLogo()
        {
            // Arrange
            string version = "0.0.0.0";

            // Act
            string logo = Constants.GetFormattedLogo(version);

            // Assert
            Assert.NotNull(logo);
            Assert.Contains("Chloroplast v" + version, logo);
            Assert.Contains("▓▓", logo); // Check that logo contains expected ASCII art characters
        }

        [Fact]
        public void GetFormattedLogo_WithShortVersion_MaintainsAlignment()
        {
            // Arrange
            string version = "1.0";

            // Act
            string logo = Constants.GetFormattedLogo(version);

            // Assert
            Assert.NotNull(logo);
            Assert.Contains("Chloroplast v" + version, logo);
            
            // Verify the version line has the correct length (should match other centered lines)
            var lines = logo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var versionLine = lines.FirstOrDefault(l => l.Contains("Chloroplast"));
            Assert.NotNull(versionLine);
            
            // Version line should be 76 characters (same as original)
            Assert.Equal(76, versionLine.Length);
        }

        [Fact]
        public void GetFormattedLogo_WithLongVersion_MaintainsAlignment()
        {
            // Arrange
            string version = "1.2.3.4567";

            // Act
            string logo = Constants.GetFormattedLogo(version);

            // Assert
            Assert.NotNull(logo);
            Assert.Contains("Chloroplast v" + version, logo);
            
            // Verify the version line has the correct length
            var lines = logo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var versionLine = lines.FirstOrDefault(l => l.Contains("Chloroplast"));
            Assert.NotNull(versionLine);
            
            // Version line should be 76 characters (same as original)
            Assert.Equal(76, versionLine.Length);
        }

        [Fact]
        public void GetFormattedLogo_VersionLineCentered()
        {
            // Arrange
            string shortVersion = "1.0";
            string longVersion = "1.2.3.4567";

            // Act
            string shortLogo = Constants.GetFormattedLogo(shortVersion);
            string longLogo = Constants.GetFormattedLogo(longVersion);

            // Assert - Extract the version line (line with "Chloroplast v")
            var shortLines = shortLogo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var longLines = longLogo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var shortVersionLine = shortLines.FirstOrDefault(l => l.Contains("Chloroplast v"));
            var longVersionLine = longLines.FirstOrDefault(l => l.Contains("Chloroplast v"));

            Assert.NotNull(shortVersionLine);
            Assert.NotNull(longVersionLine);

            // Both lines should have the same total length
            Assert.Equal(shortVersionLine.Length, longVersionLine.Length);
        }

        [Fact]
        public void GetFormattedLogo_WithVariousVersions_AllHaveSameWidth()
        {
            // Arrange
            var versions = new[] { "1.0", "1.2.3", "0.0.0.0", "10.20.30.4000", "2.1" };

            // Act & Assert
            foreach (var version in versions)
            {
                var logo = Constants.GetFormattedLogo(version);
                var lines = logo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Find the version line
                var versionLine = lines.FirstOrDefault(l => l.Contains("Chloroplast"));
                Assert.NotNull(versionLine);
                
                // All version lines should have the same width (76 characters)
                Assert.Equal(76, versionLine.Length);
            }
        }

        [Fact]
        public void GetFormattedLogo_ContainsExpectedStructure()
        {
            // Arrange
            string version = "1.0.0";

            // Act
            string logo = Constants.GetFormattedLogo(version);

            // Assert - Verify the logo contains key structural elements
            Assert.Contains("▄", logo);
            Assert.Contains("▐▓▓▓▌", logo);
            Assert.Contains("▒▓▓▓▓▓▒", logo);
            Assert.Contains("Chloroplast v" + version, logo);
            
            // Should have 19 lines total
            var lines = logo.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(19, lines.Length);
        }
    }
}
