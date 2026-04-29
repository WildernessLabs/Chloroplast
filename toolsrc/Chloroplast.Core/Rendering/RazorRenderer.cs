using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;
using RazorLight;

namespace Chloroplast.Core.Rendering
{
    public class RazorRenderer
    {
        public static RazorRenderer Instance;

        IRazorLightEngine engine;
        Dictionary<string, string> templateSources = new Dictionary<string, string> ();
        string templatesFolderPath;

        public async Task AddTemplateAsync (string templatePath, string templatesFolderPath)
        {
            string fileName = Path.GetFileNameWithoutExtension (templatePath);
            
            // Calculate the relative path from templates folder
            string relativePath = Path.GetRelativePath(templatesFolderPath, templatePath);
            // Normalize to forward slashes and remove .cshtml extension
            string relativeKey = relativePath.Replace('\\', '/').Replace(".cshtml", "");
            
            // Store with both the relative path and the filename for backward compatibility
            string source = await File.ReadAllTextAsync (templatePath);
            
            // Store by relative path (e.g., "template/topNav")
            if (!templateSources.ContainsKey (relativeKey))
            {
                templateSources[relativeKey] = source;
            }
            
            // Also store by filename only for backward compatibility (e.g., "topNav")
            // But only if there's no conflict
            if (!templateSources.ContainsKey (fileName))
            {
                templateSources[fileName] = source;
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

            engine = new RazorLightEngineBuilder ()
                .UseMemoryCachingProvider ()
                .SetOperatingAssembly (typeof (RazorRenderer).Assembly)
                .Build ();

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
                string frameName = parsed.Metadata?["frame"] ?? "SiteFrame";
                if (string.IsNullOrEmpty(frameName))
                {
                    frameName = "SiteFrame";
                }

                // Try to find the frame template
                string key = FindKey(frameName);
                
                if (key == null)
                {
                    // Log error and return null to signal the file should be skipped
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: Frame template '{frameName}' not found for content '{parsed.Node?.Title ?? "unknown"}'. Skipping this file.");
                    Console.ResetColor();
                    return null;
                }

                var result = await engine.CompileRenderStringAsync<FrameRenderedContent> (key, templateSources[key], parsed);
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
            string key = FindKey(templateName);
            if (key == null)
            {
                // Template not found - log warning and return empty string instead of throwing
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Template '{templateName}' not found. Rendering empty content.");
                Console.ResetColor();
                return new RawString(string.Empty);
            }
            return new RawString (await engine.CompileRenderStringAsync<T> (key, templateSources[key], model));
        }

        public bool TemplateExists(string templateName)
        {
            return FindKey(templateName) != null;
        }

        private string FindKey(string templateName)
        {
            // Try multiple lookup strategies to find the template
            
            // 1. Try exact match (could be relative path like "template/topNav")
            if (templateSources.ContainsKey(templateName))
            {
                return templateName;
            }

            // 2. Try with .cshtml extension if not already present
            if (!templateName.EndsWith(".cshtml"))
            {
                string withExtension = templateName + ".cshtml";
                string normalized = withExtension.Replace('\\', '/').Replace(".cshtml", "");
                if (templateSources.ContainsKey(normalized))
                {
                    return normalized;
                }
            }

            // 3. Try removing .cshtml if present
            if (templateName.EndsWith(".cshtml"))
            {
                string withoutExtension = templateName.Substring(0, templateName.Length - 7);
                string normalized = withoutExtension.Replace('\\', '/');
                if (templateSources.ContainsKey(normalized))
                {
                    return normalized;
                }
            }

            // 4. Normalize path separators and try again
            string normalizedName = templateName.Replace('\\', '/');
            if (templateSources.ContainsKey(normalizedName))
            {
                return normalizedName;
            }

            // 5. Last resort: if path starts with "templates/", strip it and try all lookups again
            // This helps users who mistakenly include the templates folder in their path
            if (normalizedName.StartsWith("templates/", StringComparison.OrdinalIgnoreCase))
            {
                string withoutTemplatesPrefix = normalizedName.Substring("templates/".Length);
                return FindKey(withoutTemplatesPrefix); // Recursive call with stripped path
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

                string key = FindKey(templateName);

                if (key == null)
                    key = FindKey(defaultTemplateName);

                // Render template
                var result = await engine.CompileRenderStringAsync<RenderedContent> (key, templateSources[key], parsed);

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

                string key = FindKey(templateName) ?? templateName;

                // Render template
                var result = await engine.CompileRenderStringAsync<EcmaXmlContent<Chloroplast.Core.Loaders.EcmaXml.Namespace>> (key, templateSources[key], parsed);

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

                string key = FindKey(templateName) ?? templateName;

                // Render template
                var result = await engine.CompileRenderStringAsync<EcmaXmlContent<Chloroplast.Core.Loaders.EcmaXml.XType>> (key, templateSources[key], parsed);

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
