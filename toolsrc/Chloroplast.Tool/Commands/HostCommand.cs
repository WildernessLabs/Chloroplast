using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Chloroplast.Core;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Chloroplast.Tool.Commands
{
    public class HostCommand : ICliCommand
    {
        string rootPath;
        string pathToUse;
        IConfigurationRoot rootconfig;
        List<FileSystemWatcher> watchers = new List<FileSystemWatcher> ();
        List<string> areaSourcePaths = new List<string> ();

        public HostCommand ()
        {
        }

        public string Name => "Host";

        public async Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
        {
            this.rootconfig = config;
            var outPath = config["out"].NormalizePath ();
            this.rootPath = config["root"].NormalizePath ();

            Console.WriteLine ($"out: " + outPath);
            Console.WriteLine ($"root: " + rootPath);

            if (!string.IsNullOrWhiteSpace(outPath))
            {
                pathToUse = outPath;
            }
            else if (!string.IsNullOrWhiteSpace(rootPath))
            {
                pathToUse = rootPath;
            }                
            else
            {
                string potentialPath = Directory.GetCurrentDirectory ().CombinePath ("out");
                if (Directory.Exists (potentialPath))
                    pathToUse = potentialPath;
                else
                {
                    Console.WriteLine ("Can't start server, please provide `out` or `root` parameter to static files");
                    return new Task[0];
                }
            }

            var host = Host.CreateDefaultBuilder ()
                .UseContentRoot (pathToUse)
                .ConfigureWebHostDefaults (webBuilder =>
                 {
                     webBuilder.CaptureStartupErrors (true);
                     webBuilder.UseWebRoot ("/");
                     webBuilder.PreferHostingUrls (true);
                     webBuilder.UseUrls ("http://localhost:5000");
                     webBuilder.UseStartup<Startup> ();
                 })
                .UseConsoleLifetime (c => c.SuppressStatusMessages = true)
                .Build ();
            //using (host)
            {
                await host.StartAsync ();
                Console.WriteLine ($"started on http://localhost:5000 ... press any key to end");

                try
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process ();
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.FileName = "http://localhost:5000/";
                    proc.Start ();
                }
                catch (Exception ex)
                {
                    Console.WriteLine ("Error starting browser, " + ex.Message);
                }

                return new[] {Task.Factory.StartNew(() =>
                    {

                        var areaConfigs = this.rootconfig.GetSection ("areas");
                        
                        if (areaConfigs != null)
                        {
                            foreach (var areaConfig in areaConfigs.GetChildren ())
                            {
                                var areaSource = areaConfig["source_folder"];
                                this.areaSourcePaths.Add(areaSource);
                                var areaSourcePath = this.rootPath.CombinePath (areaSource);
                                Console.WriteLine ("watching: " + areaSourcePath);
                                var watcher = new FileSystemWatcher(areaSourcePath);

                                watcher.NotifyFilter = NotifyFilters.LastWrite
                                                        | NotifyFilters.Size;

                                watcher.Changed += Watcher_Changed;
                                watcher.Error += Watcher_Error;

                                // TODO: consider changing filter so we copy over static assets
                                watcher.Filter = "*.md";
                                watcher.IncludeSubdirectories = true;
                                watcher.EnableRaisingEvents = true;
                                watchers.Add(watcher);
                            }

                        }
                        else
                        {
                            Console.WriteLine("no source areas to watch for changes in SiteConfig.yml");
                        }

                        Console.WriteLine("Press Enter to Quit Host");
                        Console.ReadLine();
                        // TODO: need a better story for disposing this in case of an error
                        foreach(var w in watchers)
                        {
                            w.Changed -= Watcher_Changed;
                            w.Error -= Watcher_Error;
                            w.Dispose();
                        }
                        watchers.Clear();
                        host.Dispose();
                }) };
            }
        }

        private static void Watcher_Error (object sender, ErrorEventArgs e) =>
            PrintException (e.GetException ());

        private static void PrintException (Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine ($"Message: {ex.Message}");
                Console.WriteLine ("Stacktrace:");
                Console.WriteLine (ex.StackTrace);
                Console.WriteLine ();
                PrintException (ex.InnerException);
            }
        }

        bool running = false;

        private void Watcher_Changed (object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            if (this.running)
            {
                Console.WriteLine ($"File changed during build, not rebuilt: {e.FullPath}");
                // TODO: add to waiting queue
                return;
            }

            // TODO: implement a custom IConfigurationRoot that points to this one changed file, and call FullBuildCommand
            WatcherConfig config = new WatcherConfig ();
            config["out"] = this.pathToUse;
            config["root"] = this.rootPath;
            config["force"] = "true";


            if (string.IsNullOrEmpty (rootconfig["templates_folder"]))
            {
                string[] siteFramePaths = Directory.GetFiles (this.rootPath, "SiteFrame.cshtml", SearchOption.AllDirectories);
                if (!siteFramePaths.Any ())
                {
                    throw new ChloroplastException ($":( Could not find 'SiteFrame.cshtml' in {this.rootPath}");
                }

                config["templates_folder"] = Path.GetDirectoryName (siteFramePaths.First ());
            }
            else
                config["templates_folder"] = rootconfig["templates_folder"];

            string relativePath = e.FullPath.Replace (this.rootPath, string.Empty);

            string relativeOut = relativePath;
            if (relativeOut.EndsWith (".md"))
                relativeOut = Path.Combine (
                        Path.GetDirectoryName (relativePath),
                        "index.html"
                    );
            foreach (var areaSourcePath in this.areaSourcePaths)
            {
                relativeOut = relativeOut.Replace (areaSourcePath, string.Empty);
            }

            config.AddSection ("files", new WatcherConfig (new Dictionary<string, string>
            {
                { "source_file", relativePath },
                { "output_folder",  relativeOut }
            }));

            FullBuildCommand fullBuild = new FullBuildCommand ();

            Console.WriteLine ($"Changed: {e.FullPath}");
            this.running = true;
            var tasks = fullBuild.RunAsync (config);
            tasks.Wait ();
            this.running = false;
        }

        internal class WatcherConfig : IConfigurationRoot, IConfigurationSection
        {
            Dictionary<string, string> rootValues;
            Dictionary<string, WatcherConfig> sections = new Dictionary<string, WatcherConfig> ();

            public WatcherConfig ()
            {
                this.rootValues = new Dictionary<string, string> ();
            }

            public WatcherConfig (Dictionary<string, string> values)
            {
                this.rootValues = values;
            }

            public string this[string key]
            {
                get => this.rootValues.ContainsKey(key) ? this.rootValues[key] : string.Empty;
                set => this.rootValues[key] = value;
            }

            public void AddSection(string key, WatcherConfig value) => this.sections[key] = value;

            public IConfigurationSection GetSection (string key) => this.sections.ContainsKey(key) ? this.sections[key] : null;

            public IEnumerable<IConfigurationSection> GetChildren ()
            {
                return (new[] { this }).Union (this.sections.Values);
            }

            #region unused interface members

            public IEnumerable<IConfigurationProvider> Providers => throw new NotImplementedException ();

            string IConfigurationSection.Key => throw new NotImplementedException ();

            string IConfigurationSection.Path => throw new NotImplementedException ();

            string IConfigurationSection.Value { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }


            public IChangeToken GetReloadToken ()
            {
                throw new NotImplementedException ();
            }

            public void Reload ()
            {
                throw new NotImplementedException ();
            }
            #endregion
        }

        public class Startup
        {
            public Startup (IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            public void ConfigureServices (IServiceCollection services)
            {
            }

            public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
            {
                app.UseDeveloperExceptionPage ();

                
                app.UseFileServer (new FileServerOptions
                {
                    FileProvider = new PhysicalFileProvider (env.ContentRootPath),
                    RequestPath = ""
                });

                app.UseRouting ();

            }
        }
        
    }
}
