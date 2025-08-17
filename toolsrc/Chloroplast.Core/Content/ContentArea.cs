using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chloroplast.Core.Extensions;
using Chloroplast.Core.Loaders;
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
            if (fileConfigs != null)
            {
                foreach (var fileConfig in fileConfigs.GetChildren ())
                {
                    // Trim any leading separators to ensure these are treated as relative to configured roots
                    var sourceFile = (fileConfig["source_file"] ?? string.Empty).SanitizeRelativeSegment();
                    var outputFolder = (fileConfig["output_folder"] ?? string.Empty).SanitizeRelativeSegment(normalizePaths);

                    var area = new IndividualContentArea
                    {
                        SourcePath = rootDirectory.CombinePath(sourceFile),
                        TargetPath = outDirectory.CombinePath(outputFolder),
                        RootRelativePath = outputFolder.Replace ("index.html", "").NormalizePath (toLower: normalizePaths)
                    };

                    yield return area;
                }
            }

            // areas
            var areaConfigs = config.GetSection ("areas");

            if (areaConfigs != null)
            {
                foreach (var areaConfig in areaConfigs.GetChildren ())
                {
                    var sourceFolder = (areaConfig["source_folder"] ?? string.Empty).SanitizeRelativeSegment();
                    var outputFolder = (areaConfig["output_folder"] ?? string.Empty).SanitizeRelativeSegment(normalizePaths);

                    var area = new GroupContentArea
                    {
                        SourcePath = rootDirectory.CombinePath(sourceFolder),
                        TargetPath = outDirectory.CombinePath(outputFolder),
                        RootRelativePath = outputFolder.Replace ("index.html", "").NormalizePath (toLower: normalizePaths),
                        NormalizePaths = normalizePaths,
                        AreaType = areaConfig["type"]
                    };

                    // TODO: validate values

                    yield return area;
                }
            }
        }
    }

    public class IndividualContentArea : ContentArea
    {
        public override IList<ContentNode> ContentNodes
        {
            get
            {
                var relativePath = this.SourcePath; // For individual files, use source path as relative
                var locale = DetectLocaleFromPath(relativePath);
                
                return new[] {
                    new ContentNode
                    {
                        Slug = Path.GetDirectoryName (this.SourcePath),
                        Source = new DiskFile (this.SourcePath, this.SourcePath),
                        Target = new DiskFile (this.TargetPath, this.TargetPath),
                        Area = this,
                        Locale = locale
                    }
                }.ToList();
            }
        }
        
        /// <summary>
        /// Detects the locale from a file path based on naming conventions.
        /// Examples: guide.es.md -> "es", guide.md -> default locale
        /// </summary>
        private string DetectLocaleFromPath(string relativePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(relativePath);
            var supportedLocales = SiteConfig.SupportedLocales;
            
            // Check if filename contains a locale code before the extension
            // e.g., guide.es.md, index.fr.md
            foreach (var locale in supportedLocales)
            {
                if (fileName.EndsWith($".{locale}", StringComparison.OrdinalIgnoreCase))
                {
                    return locale;
                }
            }
            
            // Check if the file is in a locale-specific directory
            // e.g., /es/guide.md, /fr/getting-started.md
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (pathParts.Length > 0)
            {
                var firstDir = pathParts[0];
                if (supportedLocales.Contains(firstDir, StringComparer.OrdinalIgnoreCase))
                {
                    return firstDir.ToLower();
                }
            }
            
            // Default to the configured default locale
            return SiteConfig.DefaultLocale;
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
                {
                    string menuPath = string.Empty;

                    nodes = Directory
                            .GetFiles (this.SourcePath, "*.*", SearchOption.AllDirectories)
                            .OrderBy (p => p)
                            .Select (p =>
                              {
                                  var relative = p.RelativePath(SourcePath, this.NormalizePaths);
                                  var targetrelative = relative;

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
                                          var indexFile = new DiskFile (p, relative);
                                          var XmlForIndex = indexFile.ReadContentAsync ().Result;
                                          var index = EcmaXmlLoader.LoadXIndex (XmlForIndex);

                                          // TODO: 'api/' should be coming from configuration
                                          var apiRootPath = $"/api/{Path.GetFileName (Path.GetDirectoryName (p)).ToLower ()}";
                                          string docSetMenu = index.ToMenu (apiRootPath);
                                          var indexMenuPath = TargetPath.CombinePath ("menu.md");
                                          var indexMenuFile = new DiskFile (indexMenuPath, "menu.md");
                                          menuPath = indexMenuPath;
                                          indexMenuFile.WriteContentAsync (docSetMenu).Wait ();
                                      }
                                  }

                                  var targetFile = TargetPath.CombinePath (targetrelative);
                                  
                                  // Detect locale from filename (e.g., guide.es.md -> es, guide.md -> default)
                                  var locale = DetectLocaleFromPath(relative);
                                  
                                  var node = new ContentNode
                                  {
                                      Slug = Path.GetDirectoryName (relative),
                                      Source = new DiskFile (p, relative),
                                      Target = new DiskFile (targetFile, targetrelative),
                                      MenuPath = menuPath,
                                      Area = this,
                                      Locale = locale
                                  };

                                  return node;
                              }).ToList ();
                              
                    // Group translations together
                    GroupTranslations(nodes);
                }
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
        
        /// <summary>
        /// Detects the locale from a file path based on naming conventions.
        /// Examples: guide.es.md -> "es", guide.md -> default locale
        /// </summary>
        private string DetectLocaleFromPath(string relativePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(relativePath);
            var supportedLocales = SiteConfig.SupportedLocales;
            
            // Check if filename contains a locale code before the extension
            // e.g., guide.es.md, index.fr.md
            foreach (var locale in supportedLocales)
            {
                if (fileName.EndsWith($".{locale}", StringComparison.OrdinalIgnoreCase))
                {
                    return locale;
                }
            }
            
            // Check if the file is in a locale-specific directory
            // e.g., /es/guide.md, /fr/getting-started.md
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (pathParts.Length > 0)
            {
                var firstDir = pathParts[0];
                if (supportedLocales.Contains(firstDir, StringComparer.OrdinalIgnoreCase))
                {
                    return firstDir.ToLower();
                }
            }
            
            // Default to the configured default locale
            return SiteConfig.DefaultLocale;
        }
        
        /// <summary>
        /// Groups content nodes by their base content and populates the Translations property.
        /// </summary>
        private void GroupTranslations(List<ContentNode> allNodes)
        {
            // Group nodes by their base content path (without locale)
            var groups = allNodes.GroupBy(node => GetBaseContentPath(node.Source.RootRelativePath));
            
            foreach (var group in groups)
            {
                var nodesList = group.ToList();
                if (nodesList.Count <= 1) continue; // No translations
                
                // Find the default language node
                var defaultNode = nodesList.FirstOrDefault(n => n.Locale == SiteConfig.DefaultLocale) ?? nodesList.First();
                
                // Set translations for the default node
                defaultNode.Translations = nodesList.Where(n => n != defaultNode).ToArray();
                
                // For non-default nodes, create a reference back to the default
                foreach (var translatedNode in nodesList.Where(n => n != defaultNode))
                {
                    var otherTranslations = nodesList.Where(n => n != translatedNode).ToArray();
                    translatedNode.Translations = otherTranslations;
                }
            }
        }
        
        /// <summary>
        /// Gets the base content path without locale identifiers.
        /// Examples: guide.es.md -> guide.md, /es/guide.md -> /guide.md
        /// </summary>
        private string GetBaseContentPath(string relativePath)
        {
            var supportedLocales = SiteConfig.SupportedLocales;
            
            // Handle locale in filename (guide.es.md -> guide.md)
            var fileName = Path.GetFileNameWithoutExtension(relativePath);
            var extension = Path.GetExtension(relativePath);
            var directory = Path.GetDirectoryName(relativePath) ?? "";
            
            foreach (var locale in supportedLocales)
            {
                if (fileName.EndsWith($".{locale}", StringComparison.OrdinalIgnoreCase))
                {
                    var baseFileName = fileName.Substring(0, fileName.Length - locale.Length - 1);
                    return Path.Combine(directory, baseFileName + extension).Replace("\\", "/");
                }
            }
            
            // Handle locale in directory (/es/guide.md -> /guide.md)
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (pathParts.Length > 1)
            {
                var firstDir = pathParts[0];
                if (supportedLocales.Contains(firstDir, StringComparer.OrdinalIgnoreCase))
                {
                    var remainingPath = string.Join("/", pathParts.Skip(1));
                    return remainingPath;
                }
            }
            
            return relativePath.Replace("\\", "/");
        }
    }
}