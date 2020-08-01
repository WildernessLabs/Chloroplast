using System;
using Chloroplast.Core.Rendering;
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
    }
}
