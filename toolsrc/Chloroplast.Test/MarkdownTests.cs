using System;
using Chloroplast.Core.Rendering;
using Xunit;

namespace Chloroplast.Test
{
    public class MarkdownTests
    {
        [Fact]
        public void SimpleMarkdown()
        {
            MarkdownRenderer renderer = new MarkdownRenderer ();
            var actual = renderer.Render (@"one paragraph

another paragraph");

            Assert.Contains ("<p>", actual);
        }
    }
}
