using System;
namespace Chloroplast.Tool
{
    public static class Constants
    {
        private const string LogoTemplate = @"                                       ▄
                                     ▐▓▓▓▌
                                    ▒▓▓▓▓▓▒
                                   ▒▓▓▓▓▓▓▓▒
             ▄                    ▒▓▓▓▓▓▓▓▓▓▒                    ▄
            ▓▓▓                  ▒▓▓▓▓▓▓▓▓▓▓▓▒                  ▓▓▓
           ▓▓▀▓▓               ▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒               ▓▓▀▓▓
          ▓▓▀ ▀▓▓             ▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒             ▓▓▀ ▀▓▓
         ▓▓▀   ▀▓▓           ▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒           ▓▓▀   ▀▓▓
        ▓▓▀     ▀▓▓        ▐▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▌        ▓▓▀     ▀▓▓
       ▓▓▀       ▀▓▓      ▒▓▓▓▓▓▓▓▓▓▓▀  ▀▀▀█▓▓▓▓▓▓▓▓▒      ▓▓▀       ▀▓▓
      ▓▓           ▓▓    ▒▓▓▌   ▀▓▀            ▀▀▀▀▓▓▒    ▓▓           ▓▓
     ▓▓             ▓▓ ▐▓▓                           ▓▓▌ ▓▓             ▓▓
    ▓▓               ▓▓▓▓▓                            ▓▓▓▓               ▓▓
   ▓▓                 ▓▓▓{0}▓▓                 ▓▓
  ▓▓                   ▓▓                             ▓▓                   ▓▓
 ▓▓                     ▓▓                           ▓▓                     ▓▓
▐▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▌                         ▐▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▌
            ▓▓                                                   ▓▓";

        public static string Logo => GetFormattedLogo("0.0.0.0");

        /// <summary>
        /// Formats the Chloroplast logo with the specified version string, maintaining proper alignment
        /// by adaptively padding whitespace around the version.
        /// </summary>
        /// <param name="version">The version string to embed in the logo</param>
        /// <returns>The formatted logo with the version centered</returns>
        public static string GetFormattedLogo(string version)
        {
            const string prefix = "Chloroplast v";
            // The total width available for padding + version text
            // Original: "     Chloroplast v0.0.0.0     " = 30 characters
            const int paddingAreaWidth = 30;
            
            string versionText = prefix + version;
            int textLength = versionText.Length;
            
            // Calculate padding needed on each side to center the text within the padding area
            int totalPadding = paddingAreaWidth - textLength;
            int leftPadding = totalPadding / 2;
            int rightPadding = totalPadding - leftPadding;
            
            // Ensure minimum padding of at least 1 space on each side
            if (leftPadding < 1) leftPadding = 1;
            if (rightPadding < 1) rightPadding = 1;
            
            // Build the padded version text that will fit in the 30-char padding area
            string paddedVersion = new string(' ', leftPadding) + versionText + new string(' ', rightPadding);
            
            return string.Format(LogoTemplate, paddedVersion);
        }
    }
}
