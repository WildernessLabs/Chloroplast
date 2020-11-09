using System;
using System.Collections.Generic;
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
            }

            return tasks;
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
    }
}
