using Xunit;
using Chloroplast.Core.Rendering;
using Chloroplast.Core;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Chloroplast.Test
{
    public class LocalizedUrlTests
    {
        class TestRenderedContent : RenderedContent { }
        class TestTemplate : ChloroplastTemplateBase<TestRenderedContent>
        {
            public TestTemplate(TestRenderedContent model) { this.Model = model; }
            public string Expose(string path, string locale) => BuildPrettyLocalizedUrl(path, locale);
            public override System.Threading.Tasks.Task ExecuteAsync() => System.Threading.Tasks.Task.CompletedTask;
        }

        [Fact]
        public void RemovesIndexAndHtml_DefaultLocale()
        {
            SiteConfig.Instance = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"defaultLocale","en"}}).Build();
            var t = new TestTemplate(new TestRenderedContent());
            var url = t.Expose("cli/index.html", "en");
            Assert.Equal("/cli/", url);
        }

        [Fact]
        public void PrefixesLocaleForNonDefault()
        {
            SiteConfig.Instance = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"defaultLocale","en"}}).Build();
            var t = new TestTemplate(new TestRenderedContent());
            var url = t.Expose("cli/index.html", "es");
            Assert.Equal("/es/cli/", url);
        }

        [Fact]
        public void RootIndexBecomesSlash()
        {
            SiteConfig.Instance = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"defaultLocale","en"}}).Build();
            var t = new TestTemplate(new TestRenderedContent());
            var url = t.Expose("index.html", "en");
            Assert.Equal("/", url);
        }

        [Fact]
        public void RootIndexNonDefaultGetsLocaleFolder()
        {
            SiteConfig.Instance = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>{{"defaultLocale","en"}}).Build();
            var t = new TestTemplate(new TestRenderedContent());
            var url = t.Expose("index.html", "es");
            Assert.Equal("/es/", url);
        }
    }
}
