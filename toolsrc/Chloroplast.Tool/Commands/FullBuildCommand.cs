using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Chloroplast.Core;
using Chloroplast.Core.Content;
using Chloroplast.Core.Extensions;
using Chloroplast.Core.Rendering;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Tool.Commands
{
    public class FullBuildCommand : ICliCommand
    {
        public FullBuildCommand ()
        {
        }

        public string Name => "Build";

        public async Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
        {
            // Set up build version for cache busting
            SetupBuildVersion(config);
            
            await ContentRenderer.InitializeAsync (config);

            List<Task<Task<RenderedContent>>> tasks= new List<Task<Task<RenderedContent>>>();

            // Start iterating over content
            foreach (var area in ContentArea.LoadContentAreas (config))
            {
                Console.WriteLine ($"Processing area: {area.SourcePath}");

                List<Task<Task<RenderedContent>>> firsttasks = new List<Task<Task<RenderedContent>>> ();
                foreach (var item in area.ContentNodes)
                {
                    if (config["force"] == string.Empty &&
                        item.Source.LastUpdated <= item.Target.LastUpdated) {
                        Console.WriteLine($"\tskipping: {item.Source.RootRelativePath}");
                        continue;
                    }

                    // TODO: refactor this out to a build queue
                    firsttasks.Add(Task.Factory.StartNew(async () =>
                    {
                        Console.WriteLine ($"\tdoc: {item.Source.RootRelativePath}");

                        if (item.Source.RootRelativePath.EndsWith(".md"))
                        {
                            var r = await ContentRenderer.FromMarkdownAsync(item);
                            r = await ContentRenderer.ToRazorAsync(r);
                            //await item.Target.WriteContentAsync(r.Body);

                            return r;
                        }
                        else if (item.Source.RootRelativePath.EndsWith(".xml"))
                        {
                            var r = await ContentRenderer.FromEcmaXmlAsync (item, config);
                            //r = await ContentRenderer.ToRazorAsync (r);

                            return r;
                        }
                        else
                        {
                            item.Source.CopyTo(item.Target);
                            return null;
                        }
                    }));
                }

                Task.WaitAll (firsttasks.ToArray ());
                var rendered = firsttasks
                    .Select (t => t.Result.Result)
                    .Where(r => r != null);

                IEnumerable<ContentNode> menutree;

                if (area is GroupContentArea)
                {
                    menutree = ((GroupContentArea)area).BuildHierarchy ().ToArray ();
                }
                else // this isn't used for frame rendering anyways. TODO: refactor this out
                    menutree = new ContentNode[0];

                // this is an experimental feature that is, for now, disabled.
                // helpful for quickly bootstrapping menu files, but should be
                // fleshed out into a full subcommand at some point
                bool bootstrapMenu = false;
                if (bootstrapMenu)
                {
                    var menus =  PrepareMenu (area.TargetPath, menutree);
                    
                    YamlRenderer.RenderAndSaveMenu (area.SourcePath.CombinePath("menu.md"), menus);
                }

                foreach (var item in rendered.Select(r => new FrameRenderedContent(r, menutree)))
                {
                    tasks.Add (Task.Factory.StartNew (async () =>
                    {
                        Console.WriteLine ($"\tframe rendering: {item.Node.Title}");

                        var result = await ContentRenderer.ToRazorAsync (item);
                        await item.Node.Target.WriteContentAsync (result.Body);

                        return result;
                            
                    }));
                }

                Task.WaitAll (tasks.ToArray ());
                
                // Generate sitemap files after all content is processed
                await GenerateSitemapsAsync(area, config);
            }

            return tasks;
        }

        private void SetupBuildVersion(IConfigurationRoot config)
        {
            // Check if cache busting is enabled (default to true)
            var cacheBustingEnabled = config.GetBool("cacheBusting:enabled", defaultValue: true);
            
            if (!cacheBustingEnabled)
            {
                Console.WriteLine("Cache busting disabled in configuration");
                return;
            }

            // Get buildVersion from command line or use timestamp default
            var buildVersion = config["buildVersion"];
            if (string.IsNullOrWhiteSpace(buildVersion))
            {
                buildVersion = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            }

            // Store the build version for templates to access
            SiteConfig.BuildVersion = buildVersion;
            
            Console.WriteLine($"Using build version for cache busting: {buildVersion}");
        }

        private IEnumerable<MenuNode> PrepareMenu (string areapath, IEnumerable<ContentNode> nodes)
        {
            foreach (var node in nodes)
            {
                string title = node.Title;

                if (string.IsNullOrEmpty(node.Title))
                {
                    title = node.Slug.GetPathFileName ().Replace("_", " ");

                    if (string.IsNullOrWhiteSpace (title))
                        continue;
                }

                var menu = new MenuNode
                {
                    Title = title,
                    Path = "/" + areapath.GetPathFileName().CombinePath(node.Slug),
                    Items = PrepareMenu (areapath, node.Children)
                };

                if (!menu.Items.Any ())
                    menu.Items = null;

                yield return menu;
            }
        }

        private async Task GenerateSitemapsAsync(ContentArea area, IConfigurationRoot config)
        {
            try
            {
                // Get sitemap configuration
                var baseUrl = config["sitemap:baseUrl"] ?? config["baseUrl"];
                var enabled = config.GetBool("sitemap:enabled", defaultValue: true);
                var maxUrlsPerSitemap = config.GetInt("sitemap:maxUrlsPerSitemap", defaultValue: 50000);

                if (!enabled || string.IsNullOrEmpty(baseUrl))
                {
                    Console.WriteLine("Sitemap generation skipped (disabled or no baseUrl configured)");
                    return;
                }

                Console.WriteLine("Generating sitemap files...");

                var generator = new SitemapGenerator(baseUrl, area.TargetPath)
                {
                    MaxUrlsPerSitemap = maxUrlsPerSitemap
                };

                var sitemapFiles = generator.GenerateSitemaps(area.ContentNodes);
                
                foreach (var file in sitemapFiles)
                {
                    Console.WriteLine($"\tgenerated: {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating sitemaps: {ex.Message}");
                // Don't fail the build if sitemap generation fails
            }
        }
    }
}
