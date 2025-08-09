using System.Threading.Tasks;
using Chloroplast.Core.Content;
using Chloroplast.Core.Extensions;
using MiniRazor;

namespace Chloroplast.Core.Rendering
{
    public abstract class ChloroplastTemplateBase<T> : TemplateBase<T> where T : RenderedContent
    {
        public ChloroplastTemplateBase ()
        {
        }

        /// <summary>
        /// Gets the build version for cache busting. Returns null if cache busting is disabled.
        /// </summary>
        protected string BuildVersion => SiteConfig.CacheBustingEnabled ? SiteConfig.BuildVersion : null;

        /// <summary>
        /// Adds a version parameter to a URL for cache busting if enabled.
        /// </summary>
        /// <param name="url">The URL to add version parameter to</param>
        /// <returns>The URL with version parameter if cache busting is enabled, otherwise the original URL</returns>
        protected string WithVersion(string url)
        {
            if (!SiteConfig.CacheBustingEnabled || string.IsNullOrWhiteSpace(SiteConfig.BuildVersion))
                return url;

            var separator = url.Contains("?") ? "&" : "?";
            return $"{url}{separator}v={SiteConfig.BuildVersion}";
        }

        protected Task<RawString> PartialAsync<K>(string templateName, K model)
        {
            return RazorRenderer.Instance.RenderTemplateContent (templateName, model);
        }

        protected async Task<RawString> PartialAsync(string menuPath)
        {
            string fullMenuPath;

            if (!menuPath.Equals (this.Model.Node.MenuPath))
            {
                fullMenuPath = SiteConfig.Instance["root"]
                .NormalizePath ()
                .CombinePath (menuPath);
            }
            else
            {
                fullMenuPath = this.Model.Node.MenuPath;
            }

            // load the menu path
            var node = new ContentNode
            {
                Slug = "/" + Model.Node.Area.TargetPath.GetPathFileName ().CombinePath (Model.Node.Slug),
                Source = new DiskFile (fullMenuPath, menuPath),
                Target = new DiskFile (fullMenuPath, menuPath),
                Parent = this.Model.Node
            };
            var r = await ContentRenderer.FromMarkdownAsync (node);
            r = await ContentRenderer.ToRazorAsync (r);

            return new RawString (r.Body);
        }
    }
}
