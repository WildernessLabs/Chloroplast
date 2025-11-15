using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chloroplast.Core;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;

namespace Chloroplast.Tool.Commands
{
    public class TemplateFileData
    {
        public string RelativeFilePath { get; set; }
        public Stream Stream { get; set; }
    }

    public interface INewTemplateFetcher
    {
        IEnumerable<TemplateFileData> GetTemplateFiles (string from);
    }

    public class NewTemplateDiskFetcher : INewTemplateFetcher
    {
        public IEnumerable<TemplateFileData> GetTemplateFiles (string from)
        {
            // get a list of files and iterate over them
            var files = Directory.GetFiles (from);
            foreach (string file in Directory.EnumerateFiles (from, "*.*", SearchOption.AllDirectories))
            {
                FileStream sourceStream = File.Open (file, FileMode.Open);
                {
                    string relativePath = file.Replace (from, string.Empty);
                    if (relativePath.StartsWith(Path.DirectorySeparatorChar))
                    {
                        relativePath = relativePath.Substring (1, relativePath.Length - 1);
                    }

                    yield return new TemplateFileData
                    {
                        RelativeFilePath = relativePath,
                        Stream = sourceStream
                    };
                }
            }
        }

    }

    public class NewCommand : ICliCommand
    {

        private IConfigurationRoot config;
        private string[] args;

        public NewCommand (string[] args)
        {
            if (args != null && args.Length < 3)
            {
                throw new ChloroplastException ($"Need two parameters for 'new' command:{Environment.NewLine}chloroplast new <template> <projectName>");
            }

            this.args = args.Skip(1).ToArray();
        }

        public string Name => "New";

        public Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
        {
            this.config = config;
            CommandLineConfigurationProvider s = config.Providers.First() as CommandLineConfigurationProvider;
            if (s == null)
            {
                throw new ApplicationException ("There's an issue with the application's configuration. Please log a bug");
            }

            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly ().Location);
            string templatesPath = Path.Combine (exePath, "ProjectTemplates");

            string from = args.First ();
            string desiredTemplatePath = Path.Combine (templatesPath, from);

            if (!Directory.Exists(desiredTemplatePath))
            {
                throw new ChloroplastException ($"Template '{from}' not found");
            }

            string to = args.Skip (1).First ();

            INewTemplateFetcher fetcher = new NewTemplateDiskFetcher ();
            

            List<Task> copyTasks = new List<Task> ();

            // get the template files, and copy them to the destination
            var templateSourceFiles = fetcher.GetTemplateFiles (desiredTemplatePath);
            foreach (TemplateFileData file in templateSourceFiles)
            {
                string destination = Path.Combine (to, file.RelativeFilePath);

                Console.WriteLine ($"Creating file: {destination}");

                destination.EnsureFileDirectory ();

                FileStream destinationStream = File.Create (destination);

                copyTasks.Add( file.Stream
                    .CopyToAsync (destinationStream)
                    .ContinueWith(t =>
                    {
                        t.Wait ();
                        file.Stream.Dispose ();
                        destinationStream.Dispose ();
                    }));
                
            }

            return Task.FromResult(copyTasks.AsEnumerable());
        }



        public string AskValue (string key)
        {
            string value = this.config[key];
            if (string.IsNullOrWhiteSpace (value))
            {
                Console.WriteLine ($"{key}: ");
                value = Console.ReadLine ();
            }
            return value;
        }
    }
}
