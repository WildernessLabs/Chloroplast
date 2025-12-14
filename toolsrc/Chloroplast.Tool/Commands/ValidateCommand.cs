using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Tool.Commands
{
    public class ValidateCommand : ICliCommand
    {
        public string Name => "Validate";

        public Task<IEnumerable<Task>> RunAsync(IConfigurationRoot config)
        {
            var outPath = config["out"]?.NormalizePath();
            
            if (string.IsNullOrWhiteSpace(outPath))
            {
                outPath = Directory.GetCurrentDirectory().CombinePath("out");
            }

            if (!Directory.Exists(outPath))
            {
                Console.Error.WriteLine($"Output directory not found: {outPath}");
                Console.Error.WriteLine("Please run 'chloroplast build' first to generate the site output.");
                return Task.FromResult<IEnumerable<Task>>(new Task[0]);
            }

            Console.WriteLine($"Validating site output at: {outPath}");
            
            var basePath = config["basePath"];
            var validator = new SiteValidator(outPath, basePath);
            validator.Validate();
            validator.WriteIssuesToConsole();

            return Task.FromResult<IEnumerable<Task>>(new Task[0]);
        }
    }
}
