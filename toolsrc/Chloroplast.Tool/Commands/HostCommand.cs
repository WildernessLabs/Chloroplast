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
        public HostCommand ()
        {
        }

        public string Name => "Host";

        public async Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
        {
            string pathToUse;
            var outPath = config["out"].NormalizePath ();
            var rootPath = config["root"].NormalizePath ();

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
            using (host)
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

                Console.Read ();
                return new Task[0];
            }
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
