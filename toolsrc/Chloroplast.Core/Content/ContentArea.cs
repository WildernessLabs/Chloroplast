using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Core.Content
{
    public class ContentArea
    {
        public ContentArea ()
        {
        }

        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        public IEnumerable<ContentNode> ContentNodes
        {
            // TODO: build the hierarchy
            get => Directory
                .GetFiles (this.SourcePath, "*.*", SearchOption.AllDirectories)
                .Select (p =>
                  {
                      var relative = p.RelativePath (SourcePath);
                      var targetrelative = relative;

                      if (targetrelative.EndsWith(".md"))
                        targetrelative = targetrelative.Substring(0, targetrelative.Length-3) + ".html";

                      var targetFile = TargetPath.CombinePath (targetrelative);
                      var node = new ContentNode
                      {
                          Slug = Path.GetFileName (Path.GetDirectoryName (p)),
                          Source = new DiskFile (p, relative),
                          Target = new DiskFile (targetFile, targetrelative)
                      };

                      return node;
                  });
        }

        public static IEnumerable<ContentArea> LoadContentAreas(IConfigurationRoot config)
        {
            var areaConfigs = config.GetSection ("areas");
            
            string rootDirectory = config["root"].NormalizePath();
            string outDirectory = config["out"].NormalizePath ();

            foreach (var areaConfig in areaConfigs.GetChildren())
            {
                var area = new ContentArea
                {
                    SourcePath = rootDirectory.CombinePath(areaConfig["source_folder"]),
                    TargetPath = outDirectory.CombinePath(areaConfig["output_folder"])
                };

                // TODO: validate values

                yield return area;
            }
        }
    }
}
