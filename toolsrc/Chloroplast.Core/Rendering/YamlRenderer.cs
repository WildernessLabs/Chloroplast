using System;
using System.Collections.Generic;
using System.IO;
using Chloroplast.Core.Config;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Chloroplast.Core.Rendering
{
    public class YamlRenderer
    {

        public (IConfigurationRoot,string) ParseDoc(string content)
        {
            (string yaml, string markdown) = Split (content);
            SiteConfigurationFileParser configParser = new SiteConfigurationFileParser ();

            ConfigurationBuilder builder = new ConfigurationBuilder ();
            builder.AddChloroplastFrontMatter (yaml);
            return (builder.Build (), markdown);
        }

        private (string yaml, string markdown) Split (string content)
        {
            // first normalize to neutralize git derpery
            content = content.Replace ("\r\n", "\n");
            var lines = content.Split ('\n');

            bool markerStarter = content.StartsWith ("---");
            int startdelimiter = markerStarter ? 1 : 0;
            int enddelimiter = 0;
            for (int i = startdelimiter; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (markerStarter)
                {
                    // just look for the closing `---`
                    if (line.StartsWith ("---"))
                    {
                        enddelimiter = i;
                        break;
                    }
                }
                else
                {
                    // there was no markerStarter ... just look for the
                    // first empty line
                    if (line.Length == 0)
                    {
                        enddelimiter = i;
                        break;
                    }
                }
            }

            // if no end delimiter was found, just return content as is
            // with no markdown
            if (enddelimiter == 0)
                return (string.Empty, content);

            string parsedYaml = lines.StringJoinFromSubArray (Environment.NewLine, startdelimiter, enddelimiter);
            string parsedMarkdown = lines.StringJoinFromSubArray (Environment.NewLine, enddelimiter+1, content.Length - enddelimiter+1);
            return (parsedYaml, parsedMarkdown);
        }

        /// <summary>
        /// Saves a menu markdown file
        /// </summary>
        /// <param name="filePath">the path, including markdown filename</param>
        /// <param name="nodes">the menu nodes we want to be included</param>
        public static void RenderAndSaveMenu(string filePath, IEnumerable<MenuNode> nodes)
        {
            var serializer = new SerializerBuilder ()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
            string fileContent = serializer.Serialize (new
            {
                template = "menu",
                navTree = nodes
            });

            string mdContent = $"---{Environment.NewLine}{fileContent}{Environment.NewLine}---{Environment.NewLine}";
            File.WriteAllText (filePath, mdContent);
        }
    }
}
