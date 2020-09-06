using System;
using System.Threading.Tasks;
using Chloroplast.Core.Rendering;
using Xunit;

namespace Chloroplast.Test
{
    public class RazorTests
    {
        //[Fact]
        public async Task SimpleRender()
        {
            RazorRenderer renderer = new RazorRenderer ();
            
            var content = await renderer.RenderContentAsync (MakeContent ());

            // TODO: need to mock up initialization
        }

        private RenderedContent MakeContent ()
        {
            throw new NotImplementedException ();
        }
    }
}
