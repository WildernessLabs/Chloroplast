using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Chloroplast.Core;
using Chloroplast.Core.Extensions;

namespace Chloroplast.Tool.Commands
{
    public class InitCommand : ICliCommand
    {
        private IConfigurationRoot config;

        public InitCommand ()
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

        public async Task WriteFile(string path, string content)
        {
            Console.WriteLine ($"writing {path}");
            await File.WriteAllTextAsync (path, content);
        }

        public string AskValue (string key)
        {
            string value = this.config[key];
            if (string.IsNullOrWhiteSpace(value))
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
