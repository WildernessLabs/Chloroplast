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

namespace Chloroplast.Tool.Commands
{
    public class HostCommand : ICliCommand
    {
        string rootPath;

        public HostCommand ()
        {
        }

        public string Name => "Host";

        public async Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
        {
            string pathToUse;
            var outPath = config["out"].NormalizePath ();
            this.rootPath = config["root"].NormalizePath ();

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
                .UseConsoleLifetime ()
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

                        Console.WriteLine ("watching: " + this.rootPath);
                        using var watcher = new FileSystemWatcher(this.rootPath);

                        watcher.NotifyFilter = NotifyFilters.LastWrite
                                             | NotifyFilters.Size;

                        watcher.Changed += Watcher_Changed;
                        watcher.Error += Watcher_Error;;

                        watcher.Filter = "*.md";
                        watcher.IncludeSubdirectories = true;
                        watcher.EnableRaisingEvents = true;

                        Console.ReadLine();

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


        private void Watcher_Changed (object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine ($"Changed: {e.FullPath}");
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
