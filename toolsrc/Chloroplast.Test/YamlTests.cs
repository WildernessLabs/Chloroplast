using System;
using Chloroplast.Core.Rendering;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders;
using Xunit;

namespace Chloroplast.Test
{
    public class YamlTests
    {
        [Fact]
        public void TestSimple()
        {
            YamlRenderer renderer = new YamlRenderer();
            (var yaml, var markdown) = renderer.ParseDoc (@"---
template: Home
title: Chloroplast Home
subtitle: Documentation site for Chloroplast.
---

# Welcome!

To the chloroplast docs. yay!");

            Assert.True (markdown.StartsWith ("# Welcome!"), "markdown parsed out");
            Assert.Equal ("Home", yaml["template"]);
            Assert.Equal ("Chloroplast Home", yaml["title"]);
            Assert.Equal ("Documentation site for Chloroplast.", yaml["subtitle"]);
        }

        [Fact]
        public void TestConfig()
        {
            string yml = @"---
# Site configuration file sample for chloroplast

# site basics
title: Chloroplast Docs
email: hello@wildernesslabs.co
description: >-
  Chloroplast by Wilderness Labs docs.

# razor templates
templates_folder: Templates

files:
  - source_file: /Docs/index.md
    output_folder: /

# main site folder
areas:
  - source_folder: /Docs
    output_folder: /
";

            InMemoryFileProvider s = new InMemoryFileProvider() ;
            s.Directory.AddFile ("/some/path/", new StringFileInfo (yml, "SiteConfig.yml"));

            var config = new ConfigurationBuilder ()
                .AddChloroplastConfig (s, "/some/path/SiteConfig.yml", false, false)
                .Build();

            Assert.Equal ("Chloroplast Docs", config["title"]);
        }

    }
}
