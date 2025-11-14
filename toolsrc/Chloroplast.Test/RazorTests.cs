using System;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Rendering;
using Xunit;

namespace Chloroplast.Test
{
    public class RazorTests
    {
        [Fact]
        public async Task MissingTemplate_ReturnsEmptyStringAndLogsWarning()
        {
            // Arrange
            RazorRenderer renderer = new RazorRenderer();
            string missingTemplateName = "NonExistentTemplate";
            var model = new { TestProperty = "TestValue" };
            
            // Capture console output
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            var result = await renderer.RenderTemplateContent(missingTemplateName, model);

            // Restore console output
            Console.SetOut(originalOut);
            var consoleOutput = stringWriter.ToString();

            // Assert
            Assert.Equal(string.Empty, result.ToString());
            Assert.Contains("Warning: Template 'NonExistentTemplate' not found", consoleOutput);
        }

        [Fact]
        public async Task MissingLocalizedTemplate_ReturnsEmptyStringAndLogsWarning()
        {
            // Arrange
            RazorRenderer renderer = new RazorRenderer();
            string missingTemplateName = "TranslationWarning";
            var model = new { TestProperty = "TestValue" };
            
            // Capture console output
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act - Call with missing template (not added via AddTemplateAsync)
            var result = await renderer.RenderTemplateContent(missingTemplateName, model);

            // Restore console output
            Console.SetOut(originalOut);
            var consoleOutput = stringWriter.ToString();

            // Assert - Should return empty string instead of throwing KeyNotFoundException
            Assert.Equal(string.Empty, result.ToString());
            Assert.Contains("Warning: Template 'TranslationWarning' not found", consoleOutput);
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
