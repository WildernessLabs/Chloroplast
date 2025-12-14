using System;
using System.Diagnostics;
using System.IO;
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
            string versionString = typeof (Program).Assembly.GetName ().Version.ToString ();

            Console.WriteLine (Constants.Logo.Replace("0.0.0.0", versionString));

            // if the only thing the user wanted to know is the version, leave it at this
            if (args.Length == 1 && args[0].EndsWith ("version", StringComparison.CurrentCultureIgnoreCase))
                return;

            if (args.Length < 1 || args.Length == 1 && (args[0] == "build" || args[0] == "host" || args[0] == "validate"))
            {
                string configPath = Directory.GetCurrentDirectory ().CombinePath ("SiteConfig.yml");
                Console.WriteLine ($"looking for {configPath}");
                if (File.Exists(configPath))
                {
                    args = new string[]
                    {
                        args.Length >= 1 ? args[0] : "build",
                        "--root",
                        Directory.GetCurrentDirectory(),
                        "--out",
                        Directory.GetCurrentDirectory().CombinePath("out").EnsureDirectory()
                    };
                }
                else
                {
                    Console.Error.WriteLine ("config not found");
                    return;
                }
            }

            var subtask = args.First ();
            var config = ParseConfig (args);

            ICliCommand command = new ConfigCommand ();

            Console.WriteLine ($"sub command: {subtask}");
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
                    case "new":
                        command = new NewCommand (args);
                        break;
                    case "validate":
                        command = new ValidateCommand ();
                        break;
                    default:
                        throw new ChloroplastException ("usage: pass 'build', 'watch', 'host', 'new', or 'validate'");
                }

                Stopwatch stopwatch = new Stopwatch ();
                stopwatch.Start ();

                Console.WriteLine ($"Running {command.Name} command");
                var childTasks = await command.RunAsync (config);
                Task.WaitAll(childTasks.ToArray());
                
                // Check for any unhandled faulted tasks (but be less verbose since we now collect errors)
                var faultedTasks = childTasks.Where(t => t.Status == TaskStatus.Faulted).ToArray();
                if (faultedTasks.Any())
                {
                    Console.Error.WriteLine($"Build completed with {faultedTasks.Length} task failure(s).");
                    
                    // Only show detailed task exceptions if it's not a build command (which handles its own errors)
                    if (!(command is FullBuildCommand))
                    {
                        foreach(var t in faultedTasks.Select(ct => ct.Exception))
                        {
                            Console.Error.WriteLine (t);
                        }
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
                Console.Error.WriteLine (ex.ToString());
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

            var builder = new ConfigurationBuilder ()
                .AddCommandLine (subtaskargs);

            if (subtask == "build" || subtask == "host")
            {
                // Find the --root parameter value, or use positional arg
                string sitePath = null;
                var rootIndex = Array.FindIndex(subtaskargs, a => a == "--root");
                if (rootIndex >= 0 && rootIndex + 1 < subtaskargs.Length)
                {
                    sitePath = subtaskargs[rootIndex + 1].NormalizePath();
                }
                else if (subtaskargs.Length > 0 && !subtaskargs[0].StartsWith("--"))
                {
                    // Fallback to positional argument
                    sitePath = subtaskargs[0].NormalizePath();
                }
                
                if (sitePath == null)
                {
                    throw new ChloroplastException("No root path specified. Use --root <path> or provide as positional argument");
                }
                
                if (!System.IO.Directory.Exists (sitePath))
                {
                    throw new ChloroplastException ("path doesn't exist: " + sitePath);
                }

                builder.AddChloroplastConfig (sitePath.CombinePath ("SiteConfig.yml"));
            }
                
            var config = builder.Build ();
            SiteConfig.Instance = config;

            // Runtime flag: --no-basepath to temporarily ignore configured basePath/baseUrl.
            // For 'host' command, automatically disable basePath for local development (issue #63)
            // unless explicitly overridden by user
            bool hasNoBasePathFlag = subtaskargs.Any(a => string.Equals(a, "--no-basepath", StringComparison.OrdinalIgnoreCase));
            bool hasBasePathFlag = subtaskargs.Any(a => string.Equals(a, "--basepath", StringComparison.OrdinalIgnoreCase));
            
            if (hasNoBasePathFlag || (subtask == "host" && !hasBasePathFlag))
            {
                SiteConfig.DisableBasePath = true;
                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (subtask == "host" && !hasNoBasePathFlag)
                    {
                        Console.WriteLine("Host command: automatically disabling basePath for local development.");
                        Console.WriteLine("(Use --basepath to override this behavior)");
                    }
                    else
                    {
                        Console.WriteLine("BasePath override active: base path application disabled for this run.");
                    }
                }
                catch { }
                finally
                {
                    try { Console.ResetColor(); } catch { }
                }
            }

            return config;
        }
    }
}
