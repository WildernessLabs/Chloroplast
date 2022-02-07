using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Chloroplast.Core;
using Chloroplast.Core.Extensions;
using System.Net.Http;
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
                using (FileStream sourceStream = File.Open (file, FileMode.Open))
                {
                    yield return new TemplateFileData
                    {
                        RelativeFilePath = file,
                        Stream = sourceStream
                    };
                }
            }
        }

    }

    public class NewTemplateUrlFetcher : INewTemplateFetcher
    {
        public IEnumerable<TemplateFileData> GetTemplateFiles (string fromUrl)
        {
            using (HttpClient client = new HttpClient ())
            {
                using (Stream stream = client.GetStreamAsync (fromUrl).Result)
                {
                    // parse result stream to get list of files
                    // then iterate and return those streams
                    throw new NotImplementedException ();
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

        public async Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
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

            // TODO: make folder in working directory with `to` as name, use that path


            INewTemplateFetcher fetcher;
            // if from is a url set fetcher to NewTemplateUrlFetcher
            //if (from.IsUrl ())
            //{
            //    fetcher = new NewTemplateUrlFetcher ();
            //}
            //else
            {
                fetcher = new NewTemplateDiskFetcher ();
            }

            List<Task> copyTasks = new List<Task> ();

            string w = Path.Combine ("some", "relative", "/path");

            // get the template files, and copy them to the destination
            var templateSourceFiles = fetcher.GetTemplateFiles (desiredTemplatePath);
            foreach (TemplateFileData file in templateSourceFiles)
            {
                string fileName = Path.GetFileName (file.RelativeFilePath);
                string destination = Path.Combine (to, fileName);
                // fileName needs to be relative path from "root"
                destination.EnsureFileDirectory ();

                using (FileStream destinationStream = File.Create (destination))
                {
                    copyTasks.Add( file.Stream.CopyToAsync (destinationStream));
                }
            }

            return copyTasks;
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



/*
public class NewCommand : ICliCommand
{

    private IConfigurationRoot config;

    public NewCommand ()
    {
    }

    public string Name => "Init";

    public async Task<IEnumerable<Task>> RunAsync (IConfigurationRoot config)
    {
        this.config = config;
        string rootDir = AskValue ("root");
        if (string.IsNullOrWhiteSpace (rootDir))
            rootDir = Directory.GetCurrentDirectory ();

        rootDir.EnsureDirectory ();

        // make the config file
        string title = AskValue ("title");
        string description = AskValue ("description");
        string siteConfigPath = rootDir.CombinePath ("SiteConfig.yml");

        string configContent = string.Format (InitConfigFile, title, description);
        await WriteFile (siteConfigPath, configContent);

        // make templates
        string templatePath = rootDir.CombinePath ("Templates").EnsureDirectory ();
        await WriteFile (templatePath.CombinePath ("Default.cshtml"), InitDefaultTemplateFile);
        await WriteFile (templatePath.CombinePath ("SiteFrame.cshtml"), InitFrameFile);

        // make default area
        string areaPath = rootDir.CombinePath ("Docs").EnsureDirectory ();
        await WriteFile (areaPath.CombinePath ("index.md"), InitDefaultIndex);

        return new[] { Task.Delay (1) };
    }

    public async Task WriteFile (string path, string content)
    {
        Console.WriteLine ($"writing {path}");
        await File.WriteAllTextAsync (path, content);
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

    private readonly string InitConfigFile = @"---
# Site configuration file sample for chloroplast

# site basics
title: {0}
description: >-
{1}

# razor templates
templates_folder: Templates

# main site folder
areas:
- source_folder: /Docs
output_folder: /
";


    private readonly string InitDefaultTemplateFile = @"
<h1>Default!</h1>

<h2>@Model.GetMeta(""title"")</h2>
@if (@Model.HasMeta(""subtitle""))
{
<h3>@Model.GetMeta(""subtitle"")</h3>
}
<div>@Raw(Model.Body)</div>";

    private readonly string InitFrameFile = @"@using System.Linq;
@using System.Collections.Generic;
@{
string RenderNodes(IEnumerable<Chloroplast.Core.ContentNode> nodes)
{
<ol>
    @foreach(var item in nodes)
    {
        <li>
            <a href=""/@item.Slug"">@item.Title</a>
            @if (item.Children.Any())
            {
                @Raw(RenderNodes(item.Children))
            }
        </li>
    }
</ol>
return """";
}
}
<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>@Model.GetMeta(""Title"")</title>
<link href=""/assets/main.css"" rel=""stylesheet"" />
</head>
<body>
<h1>Wilderness Labs</h1>
<div style=""float:left;width:50%"">
@Raw(RenderNodes(Model.Tree))
</div>
<div class=""maincontent"">
@Raw(Model.Body)
</div>
</body>
</html>
";
    private readonly string InitDefaultIndex = @"---
template: Home
title: Home
subtitle: Documentation site
---

# Welcome!

To the docs. yay!
";
}
}
}*/