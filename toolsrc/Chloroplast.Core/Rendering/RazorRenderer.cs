using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;
using MiniRazor;
using MiniRazor.Primitives;

namespace Chloroplast.Core.Rendering
{
    public class RazorRenderer
    {
        public static RazorRenderer Instance;

        Dictionary<string, MiniRazorTemplateDescriptor> templates = new Dictionary<string, MiniRazorTemplateDescriptor> ();
        MiniRazorTemplateEngine engine = new MiniRazorTemplateEngine ();

        public async Task AddTemplateAsync(string templatePath)
        {
            string fileName = Path.GetFileNameWithoutExtension (templatePath);
            templates[fileName] = engine.Compile (await File.ReadAllTextAsync (templatePath));
            
        }

        public async Task InitializeAsync (IConfigurationRoot config)
        {
            string rootPath = config["root"].NormalizePath();
            string templatePath = config["templates_folder"].NormalizePath();
            string fullTemplatePath = rootPath.CombinePath (templatePath);

            foreach (var razorPath in Directory.EnumerateFiles (fullTemplatePath, "*.cshtml", SearchOption.AllDirectories))
            {
                await this.AddTemplateAsync (razorPath);
            }

            // danger will robinson ...
            // there should be only one ... big assumption here
            Instance = this;
        }

        public async Task<string> RenderContentAsync (FrameRenderedContent parsed)
        {
            try
            {
                // now render into site frame
                var frame = templates["SiteFrame"];

                var result = await frame.RenderAsync (parsed);
                return result;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine (ex.ToString ());
                Console.ResetColor ();
                return ex.ToString ();
            }
        }

        public async Task<RawString> RenderTemplateContent<T>(string templateName, T model)
        {
            var template = templates[templateName];
            return new RawString(await template.RenderAsync (model));
        }

        public async Task<string> RenderContentAsync (RenderedContent parsed)
        {
            try
            { 
                string defaultTemplateName = "Default";
                string templateName = defaultTemplateName;
                if (parsed.Metadata.ContainsKey ("template"))
                    templateName = parsed.Metadata["template"];

                if (parsed.Metadata.ContainsKey ("layout"))
                    templateName = parsed.Metadata["layout"];

                MiniRazorTemplateDescriptor template;

                if (!templates.TryGetValue(templateName, out template))
                    template = templates[defaultTemplateName];

                // Render template
                var result = await template.RenderAsync (parsed);

                return result;

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine (ex.ToString ());
                Console.ResetColor ();
                return ex.ToString ();
            }
        }
    }
}
