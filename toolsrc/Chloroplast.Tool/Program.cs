using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Extensions;
using Chloroplast.Tool.Commands;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Tool
{
    class Program
    {
        static async Task Main (string[] args)
        {
            Console.WriteLine (Constants.Logo);

            if (args.Length < 1)
            {
                Console.Error.WriteLine ("No parameters");
                return;
            }

            var subtask = args.First ();
            var config = ParseConfig (args);

            ICliCommand command = new ConfigCommand ();

            try
            {
                switch (subtask)
                {
                    case "build":
                        command = new FullBuildCommand ();
                        break;
                    case "watch":
                        command = new WatchCommand ();
                        break;
                    case "host":
                        command = new HostCommand ();
                        break;
                    default:
                        throw new ChloroplastException ("usage: pass 'build', or 'watch'");
                }

                Stopwatch stopwatch = new Stopwatch ();
                stopwatch.Start ();

                Console.WriteLine ($"Running {command.Name} command");
                var childTasks = await command.RunAsync (config);
                Task.WaitAll(childTasks.ToArray());
                if (childTasks.Any(t=>t.Status == TaskStatus.Faulted))
                {
                    foreach(var t in childTasks.Select(ct => ct.Exception))
                    {
                        Console.WriteLine (t);
                    }
                }
                stopwatch.Stop ();
                string elapsedTime = GetElapsed (stopwatch);
                Console.WriteLine ($"{command.Name} completed run in {elapsedTime}.");
            }
            catch (ChloroplastException cex)
            {
                Console.Error.WriteLine ($"Unable to complete {command.Name}");
                Console.Error.WriteLine (cex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine ($"Oops, this was unexpected :(");
                Console.Error.WriteLine (ex.Message);
                if (ex.InnerException != null)
                {
                    var currentEx = ex.InnerException;
                    while (currentEx != null)
                    {
                        Console.Error.WriteLine ("--------------");
                        Console.Error.WriteLine (currentEx.ToString ());
                        currentEx = currentEx.InnerException;
                    }
                }
            }
        }

        private static string GetElapsed (Stopwatch stopwatch)
        {
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format ("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            return elapsedTime;
        }

        private static IConfigurationRoot ParseConfig (string[] args)
        {
            var subtask = args.First ();
            var subtaskargs = args.Skip (1).ToArray ();

            if (!subtaskargs.Any())
            {
                throw new ChloroplastException ("No path provided");
            }

            var builder = new ConfigurationBuilder ()
                .AddCommandLine (subtaskargs);

            if (subtask == "build")
            {
                var sitePath = subtaskargs.Skip (1).First ().NormalizePath ();
                if (!System.IO.Directory.Exists (sitePath))
                {
                    throw new ChloroplastException ("path doesn't exist: " + sitePath);
                }

                builder.AddChloroplastConfig (sitePath.CombinePath ("SiteConfig.yml"));
            }
                
            var config = builder.Build ();
            return config;
        }
    }
}
