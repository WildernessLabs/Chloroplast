using System;
using System.Threading.Tasks;
using Chloroplast.Core.Rendering;
using Xunit;

namespace Chloroplast.Test
{
    public class RazorTests
    {
        [Fact]
        public async Task MissingTemplate_ReturnsEmptyString()
        {
            // Arrange
            RazorRenderer renderer = new RazorRenderer();
            string missingTemplateName = "NonExistentTemplate";
            var model = new { TestProperty = "TestValue" };

            // Act
            var result = await renderer.RenderTemplateContent(missingTemplateName, model);

            // Assert
            Assert.Equal(string.Empty, result.ToString());
        }

        [Fact]
        public async Task MissingLocalizedTemplate_ReturnsEmptyString()
        {
            // Arrange
            RazorRenderer renderer = new RazorRenderer();
            string missingTemplateName = "TranslationWarning";
            var model = new { TestProperty = "TestValue" };

            // Act - Call with missing template (not added via AddTemplateAsync)
            var result = await renderer.RenderTemplateContent(missingTemplateName, model);

            // Assert - Should return empty string instead of throwing KeyNotFoundException
            Assert.Equal(string.Empty, result.ToString());
        }

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
