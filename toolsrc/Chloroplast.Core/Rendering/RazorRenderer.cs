using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;
using MiniRazor;

namespace Chloroplast.Core.Rendering
{
    public class RazorRenderer
    {
        Dictionary<string, MiniRazorTemplateDescriptor> templates = new Dictionary<string, MiniRazorTemplateDescriptor> ();
        MiniRazorTemplateEngine engine = new MiniRazorTemplateEngine ();

        public async Task AddTemplateAsync(string templatePath)
        {
            string fileName = Path.GetFileNameWithoutExtension (templatePath);
            templates[fileName] = engine.Compile (await File.ReadAllTextAsync (templatePath));
        }

        public async Task InitializeAsync (IConfigurationRoot config)
        {
            string rootPath = config["root"];
            string templatePath = config["templates_folder"];
            string fullTemplatePath = rootPath.CombinePath (templatePath);

            foreach (var razorPath in Directory.EnumerateFiles (fullTemplatePath, "*.cshtml", SearchOption.AllDirectories))
            {
                await this.AddTemplateAsync (razorPath);
            }
        }

        public async Task<string> RenderAsync (RenderedContent parsed)
        {
            string defaultTemplateName = "Default";
            string templateName = defaultTemplateName;
            if (parsed.Metadata.ContainsKey ("template"))
                templateName = parsed.Metadata["template"];

            if (parsed.Metadata.ContainsKey ("layout"))
                templateName = parsed.Metadata["layout"];

            MiniRazorTemplateDescriptor template = null;

            if (!templates.TryGetValue(templateName, out template))
                template = templates[defaultTemplateName];

            // Render template
            var result = await template.RenderAsync (parsed);

            // now render into site frame
            var frame = templates["SiteFrame"];
            parsed.Body = result;
            result = await frame.RenderAsync (parsed);

            return result;
        }
    }
}
