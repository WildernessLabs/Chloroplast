using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Core.Content
{
    public abstract class ContentArea
    {
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string RootRelativePath { get; set; }
        public abstract IList<ContentNode> ContentNodes { get; }

        public static IEnumerable<ContentArea> LoadContentAreas (IConfigurationRoot config)
        {

            string rootDirectory = config["root"].NormalizePath ();
            string outDirectory = config["out"].NormalizePath ();
            bool normalizePaths = config.GetBool ("normalizePaths", defaultValue: true);

            // individual files

            var fileConfigs = config.GetSection ("files");
            foreach (var fileConfig in fileConfigs.GetChildren ())
            {
                var area = new IndividualContentArea
                {
                    SourcePath = rootDirectory.CombinePath (fileConfig["source_file"]),
                    TargetPath = outDirectory.CombinePath (fileConfig["output_folder"].NormalizePath(toLower: normalizePaths)),
                    RootRelativePath = fileConfig["output_folder"].Replace ("index.html", "").NormalizePath(toLower: normalizePaths)
                };

                yield return area;
            }

            // areas
            var areaConfigs = config.GetSection ("areas");


            foreach (var areaConfig in areaConfigs.GetChildren ())
            {
                var area = new GroupContentArea
                {
                    SourcePath = rootDirectory.CombinePath (areaConfig["source_folder"]),

                    TargetPath = outDirectory.CombinePath (areaConfig["output_folder"].NormalizePath(toLower: normalizePaths)),
                    RootRelativePath = areaConfig["output_folder"].Replace ("index.html", "").NormalizePath (toLower: normalizePaths),
                    NormalizePaths = normalizePaths,
                    AreaType = areaConfig["type"]
                };

                // TODO: validate values

                yield return area;
            }
        }
    }

    public class IndividualContentArea : ContentArea
    {
        public override IList<ContentNode> ContentNodes
        {
            get
            {
                return new[] {
                    new ContentNode
                    {
                        Slug = Path.GetDirectoryName (this.SourcePath),
                        Source = new DiskFile (this.SourcePath, this.SourcePath),
                        Target = new DiskFile (this.TargetPath, this.TargetPath),
                        Area = this
                    }
                }.ToList();
            }
        }
    }

    public class GroupContentArea : ContentArea
    {
        List<ContentNode> nodes;
        
        public bool NormalizePaths { get; set; }

        string areaType = "markdown";
        public string AreaType
        {
            get => areaType;
            set
            {
                if (!string.IsNullOrWhiteSpace (value))
                    areaType = value;
            }
        } 

        public GroupContentArea ()
        {
        }

        public GroupContentArea (IEnumerable<ContentNode> inputNodes)
        {
            this.nodes = new List<ContentNode> (inputNodes);
        }

        public override IList<ContentNode> ContentNodes
        {
            get
            {
                if (nodes == null)
                    nodes = Directory
                        // TODO , switch this back to *.* ... *.xml is only for testing
                            .GetFiles (this.SourcePath, "*.*", SearchOption.AllDirectories)
                            .Select (p =>
                              {
                                  var relative = p.RelativePath (SourcePath);
                                  var targetrelative = relative.NormalizePath(toLower: this.NormalizePaths);

                                  if (targetrelative.EndsWith (".md"))
                                      targetrelative = targetrelative.Substring (0, targetrelative.Length - 3) + ".html";

                                  else if (targetrelative.EndsWith (".xml"))
                                  {
                                      targetrelative = targetrelative.Substring (0, targetrelative.Length - 4) + ".html";
                                      if (targetrelative.Contains ("ns-"))
                                      {
                                          // this is a namespace, let's switch the target filename
                                          var filename = Path.GetFileNameWithoutExtension (targetrelative).Replace ("ns-", string.Empty);
                                          var folder = Path.GetDirectoryName (targetrelative);
                                          targetrelative = Path.Combine (folder, filename, "index.html");
                                      }
                                      else if (targetrelative.EndsWith ("index.html"))
                                      {
                                          // let's parse this and pull out menu information
                                          
                                      }
                                  }

                                  var targetFile = TargetPath.CombinePath (targetrelative);
                                  var node = new ContentNode
                                  {
                                      Slug = Path.GetDirectoryName (relative),
                                      Source = new DiskFile (p, relative),
                                      Target = new DiskFile (targetFile, targetrelative),
                                      Area = this
                                  };

                                  return node;
                              }).ToList();
                return nodes;
            }
        }

        

        /// <summary>
        /// Using heuristics to build the content tree
        /// </summary>
        /// <returns>The content nodes, nested as they are in the file system.</returns>
        public IEnumerable<ContentNode> BuildHierarchy ()
        {
            Dictionary<string, ContentNode> items = new Dictionary<string, ContentNode> ();

            ContentNode currentParent = null;
            ContentNode previous = null;
            int nestDepth = 0;
            int topDepth = 0;

            // sort by the path, and then go up and down the tree adding to the nest
            var nodes = this.ContentNodes
                .Where (n => n.Source.RootRelativePath.EndsWith(".md"))
                .OrderBy (n => Path.GetDirectoryName(n.Source.RootRelativePath))
                .ToArray();
            foreach (var node in nodes)
            {
                int thisDepth = node.Source.RootRelativePath.Length - node.Source.RootRelativePath.Replace (Path.DirectorySeparatorChar.ToString(), string.Empty).Length;

                bool wentDeeper = thisDepth > nestDepth;
                bool wentUp = thisDepth < nestDepth;
                bool sameDepth = thisDepth == nestDepth;

                // for the first iteration, and for when the depth doesn't change but it's top level
                if (currentParent == null || ((sameDepth || wentUp) && thisDepth == topDepth))
                {
                    currentParent = node;
                    nestDepth = thisDepth;
                    topDepth = thisDepth;
                    items.Add (node.Source.RootRelativePath, node);
                }
                else
                {
                    if (wentDeeper)
                    {
                        currentParent = previous;
                    }
                    if (wentUp)
                    {
                        currentParent = currentParent.Parent;
                    }

                    node.Parent = currentParent;
                    currentParent.Children.Add (node);

                }
                
                nestDepth = thisDepth;
                previous = node;
            }

            return items.Values.Where(n => n.Parent == null);
        }
    }
}