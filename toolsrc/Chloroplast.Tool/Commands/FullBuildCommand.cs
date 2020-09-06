using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Chloroplast.Core;
using Chloroplast.Core.Content;
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

                IEnumerable<ContentNode> menutree = area.BuildHierarchy ().ToArray();

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
    }
}
