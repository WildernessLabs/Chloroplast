using System;
using System.Collections.Generic;
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

        public Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
        {
            // start the task that will load the content area, and publish to the blocks
            return Task.Factory.StartNew<IEnumerable<Task>> (() =>
             {
                 List<Task> tasks= new List<Task>();

                // Start iterating over content
                foreach (var area in ContentArea.LoadContentAreas (config))
                 {
                     Console.WriteLine ($"Processing area: {area.SourcePath}");

                    foreach (var item in area.ContentNodes)
                    {
                        if (config["force"] == string.Empty && item.Source.LastUpdated <= item.Target.LastUpdated) {
                            Console.WriteLine($"\tskipping: {item.Source.RootRelativePath}");
                            continue;
                         }

                         // TODO: refactor this out to a build queue
                         tasks.Add(Task.Factory.StartNew(async () =>
                         {
                            Console.WriteLine ($"\tdoc: {item.Source.RootRelativePath}");

                            if (item.Source.RootRelativePath.EndsWith(".md"))
                            {
                                var r = await ContentRenderer.FromMarkdownAsync(item);
                                r = await ContentRenderer.ToRazorAsync(r);
                                await item.Target.WriteContentAsync(r.Body);
                            }
                            else
                            {

                                item.Source.CopyTo(item.Target);
                            }
                         }));
                     }
                 }

                 Task.WaitAll(tasks.ToArray());
                 return tasks;
             });
        }
    }
}
