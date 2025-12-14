using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Extensions;
//using Choroplast.Core.Loaders.EcmaXml;
using Microsoft.Extensions.Configuration;
using MiniRazor;

namespace Chloroplast.Core.Rendering
{
    public class RazorRenderer
    {
        public static RazorRenderer Instance;

        Dictionary<string, TemplateDescriptor> templates = new Dictionary<string, TemplateDescriptor> ();
        string templatesFolderPath;
        //TemplateEngine engine = new TemplateEngine ();

        public async Task AddTemplateAsync (string templatePath, string templatesFolderPath)
        {
            string fileName = Path.GetFileNameWithoutExtension (templatePath);
            
            // Calculate the relative path from templates folder
            string relativePath = Path.GetRelativePath(templatesFolderPath, templatePath);
            // Normalize to forward slashes and remove .cshtml extension
            string relativeKey = relativePath.Replace('\\', '/').Replace(".cshtml", "");
            
            // Store with both the relative path and the filename for backward compatibility
            var compiledTemplate = Razor.Compile (await File.ReadAllTextAsync (templatePath));
            
            // Store by relative path (e.g., "template/topNav")
            if (!templates.ContainsKey (relativeKey))
            {
                Chloroplast.Core.Loaders.EcmaXml.Namespace ns = new Chloroplast.Core.Loaders.EcmaXml.Namespace ();
                Console.WriteLine (ns.ToString ());
                templates[relativeKey] = compiledTemplate;
            }
            
            // Also store by filename only for backward compatibility (e.g., "topNav")
            // But only if there's no conflict
            if (!templates.ContainsKey (fileName))
            {
                templates[fileName] = compiledTemplate;
            }
        }

        public async Task InitializeAsync (IConfigurationRoot config)
        {
            string rootPath = config["root"].NormalizePath ();
            var templateFolderSetting = config["templates_folder"];
            if (string.IsNullOrWhiteSpace(templateFolderSetting))
                templateFolderSetting = "templates";

            // Rely on CombinePath/NormalizePath to handle absolute/relative + separator normalization
            templatesFolderPath = rootPath
                .CombinePath(templateFolderSetting)
                .NormalizePath();

            foreach (var razorPath in Directory.EnumerateFiles (templatesFolderPath, "*.cshtml", SearchOption.AllDirectories))
            {
                await this.AddTemplateAsync (razorPath, templatesFolderPath);
            }

            // danger will robinson ...
            // there should be only one ... big assumption here
            Instance = this;
        }

        public async Task<string> RenderContentAsync (FrameRenderedContent parsed)
        {
            try
            {
                // Check for custom frame in metadata, default to "SiteFrame"
                string frameName = "SiteFrame";
                if (parsed.Metadata != null && !string.IsNullOrEmpty(parsed.Metadata["frame"]))
                {
                    frameName = parsed.Metadata["frame"];
                }

                // Try to find the frame template
                var frame = FindTemplate(frameName);
                
                if (frame == null)
                {
                    // Log error and return null to signal the file should be skipped
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: Frame template '{frameName}' not found for content '{parsed.Node?.Title ?? "unknown"}'. Skipping this file.");
                    Console.ResetColor();
                    return null;
                }

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

        public async Task<RawString> RenderTemplateContent<T> (string templateName, T model)
        {
            var template = FindTemplate(templateName);
            if (template == null)
            {
                // Template not found - log warning and return empty string instead of throwing
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Template '{templateName}' not found. Rendering empty content.");
                Console.ResetColor();
                return new RawString(string.Empty);
            }
            return new RawString (await template.RenderAsync (model));
        }

        public bool TemplateExists(string templateName)
        {
            return FindTemplate(templateName) != null;
        }

        private TemplateDescriptor FindTemplate(string templateName)
        {
            // Try multiple lookup strategies to find the template
            
            // 1. Try exact match (could be relative path like "template/topNav")
            if (templates.TryGetValue(templateName, out var template))
            {
                return template;
            }

            // 2. Try with .cshtml extension if not already present
            if (!templateName.EndsWith(".cshtml"))
            {
                string withExtension = templateName + ".cshtml";
                if (templates.TryGetValue(withExtension.Replace('\\', '/').Replace(".cshtml", ""), out template))
                {
                    return template;
                }
            }

            // 3. Try removing .cshtml if present
            if (templateName.EndsWith(".cshtml"))
            {
                string withoutExtension = templateName.Substring(0, templateName.Length - 7);
                if (templates.TryGetValue(withoutExtension.Replace('\\', '/'), out template))
                {
                    return template;
                }
            }

            // 4. Normalize path separators and try again
            string normalizedName = templateName.Replace('\\', '/');
            if (templates.TryGetValue(normalizedName, out template))
            {
                return template;
            }

            // 5. Last resort: if path starts with "templates/", strip it and try all lookups again
            // This helps users who mistakenly include the templates folder in their path
            if (normalizedName.StartsWith("templates/", StringComparison.OrdinalIgnoreCase))
            {
                string withoutTemplatesPrefix = normalizedName.Substring("templates/".Length);
                return FindTemplate(withoutTemplatesPrefix); // Recursive call with stripped path
            }

            return null;
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

                TemplateDescriptor template = FindTemplate(templateName);

                if (template == null)
                    template = FindTemplate(defaultTemplateName);

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

        public async Task<string> RenderContentAsync (EcmaXmlContent<Chloroplast.Core.Loaders.EcmaXml.Namespace> parsed)
        {
            try
            {
                string templateName = "Namespace";

                TemplateDescriptor template;

                if (!templates.TryGetValue (templateName, out template))
                    template = templates[templateName];

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

        public async Task<string> RenderContentAsync (EcmaXmlContent<Chloroplast.Core.Loaders.EcmaXml.XType> parsed)
        {
            try
            {
                string templateName = "Type";

                TemplateDescriptor template;

                if (!templates.TryGetValue (templateName, out template))
                    template = templates[templateName];

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
