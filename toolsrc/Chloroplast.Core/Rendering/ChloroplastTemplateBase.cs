using System.Linq;
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

    /// <summary>
    /// Prefixes a site-relative URL with the configured BasePath.
    /// </summary>
    protected string Href(string path) => SiteConfig.ApplyBasePath(path);

    /// <summary>
    /// Builds an asset URL with BasePath and optional cache-busting version.
    /// </summary>
    protected string Asset(string path) => WithVersion(SiteConfig.ApplyBasePath(path));
    
    /// <summary>
    /// Builds a localized URL with locale prefix and BasePath applied.
    /// For the default locale, returns the standard path without locale prefix.
    /// </summary>
    /// <param name="path">The site-relative path</param>
    /// <param name="locale">The locale code (e.g., "en", "es", "fr")</param>
    protected string LocaleHref(string path, string locale) => SiteConfig.ApplyLocalePath(path, locale);
    
    /// <summary>
    /// Gets the current page's locale.
    /// </summary>
    protected string CurrentLocale => Model?.Node?.Locale ?? SiteConfig.DefaultLocale;
    
    /// <summary>
    /// Gets whether the current page was machine translated.
    /// </summary>
    protected bool IsMachineTranslated => Model?.Node?.IsMachineTranslated ?? false;
    
    /// <summary>
    /// Gets whether the current page has a translation in the specified locale.
    /// </summary>
    /// <param name="locale">The locale to check for</param>
    protected bool HasTranslation(string locale)
    {
        if (CurrentLocale == locale) return true;
        return Model?.Node?.Translations?.Any(t => t.Locale == locale) ?? false;
    }

        /// <summary>
        /// Builds a menu item href from a raw path string.
        /// Performs:
        ///  * Normalization (leading slash, strip .html)
        ///  * Locale prefixing (if non-default and not already locale-prefixed)
        ///  * BasePath application
        /// </summary>
        /// <param name="rawPath">The raw path value from menu metadata (may be null, relative, missing leading slash).</param>
        protected string BuildMenuItemHref(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath)) return string.Empty;

            // Normalize menu path first (leading slash, remove .html)
            var normalized = rawPath.NormalizeMenuPath();

            var locale = CurrentLocale;
            var defaultLocale = SiteConfig.DefaultLocale;

            // If non-default locale and path isn't already locale-prefixed, apply locale path; else standard href
            if (locale != defaultLocale && !normalized.IsLocalePrefixed(locale))
            {
                return LocaleHref(normalized, locale);
            }

            // Already localized or default locale
            return Href(normalized);
        }
    
    /// <summary>
    /// Gets the URL for the current page in the specified locale.
    /// If the page doesn't exist in that locale, returns the default language version.
    /// </summary>
    /// <param name="locale">The target locale</param>
    protected string GetLocalizedPageUrl(string locale)
    {
        // Try to find an authored translation first
        if (Model?.Node?.Translations != null)
        {
            var translation = System.Array.Find(Model.Node.Translations, t => t.Locale == locale);
            if (translation != null)
            {
                return BuildPrettyLocalizedUrl(translation.Target.RootRelativePath, locale);
            }
        }

        // Use current node (may be fallback synthesized) and project into requested locale
        var currentRootRelative = Model?.Node?.Target?.RootRelativePath;
        if (!string.IsNullOrWhiteSpace(currentRootRelative))
        {
            return BuildPrettyLocalizedUrl(currentRootRelative, locale);
        }

        // Final fallback: root
        return BuildPrettyLocalizedUrl("index.html", locale);
    }

    /// <summary>
    /// Converts a target RootRelativePath (e.g., "cli/index.html", "es/cli/index.html", "installing.html")
    /// into a pretty, locale-aware, base-path-applied URL.
    /// Rules:
    ///  * Strip .html
    ///  * Collapse any trailing /index to just a trailing /
    ///  * For non-default locales, ensure the leading /{locale}/ prefix (unless already present)
    ///  * For default locale, ensure no locale prefix
    ///  * Always return a leading '/'
    ///  * Root index becomes '/'
    /// </summary>
    protected internal string BuildPrettyLocalizedUrl(string rootRelativePath, string locale)
    {
        if (string.IsNullOrWhiteSpace(rootRelativePath)) return SiteConfig.ApplyBasePath("/");

        var defaultLocale = SiteConfig.DefaultLocale;

        // Standardize separators
        var p = rootRelativePath.Replace('\\', '/'); // e.g. es/cli/index.html

        // Remove any leading ./ that might sneak in (defensive)
        if (p.StartsWith("./")) p = p.Substring(1);

        // Strip .html extension (only at end)
        if (p.EndsWith(".html", System.StringComparison.OrdinalIgnoreCase))
            p = p.Substring(0, p.Length - 5); // remove .html -> es/cli/index

        // Ensure leading slash for further checks
        if (!p.StartsWith('/')) p = "/" + p; // /es/cli/index OR /installing

        // If we have a trailing /index, collapse it
        if (p.EndsWith("/index", System.StringComparison.OrdinalIgnoreCase))
        {
            if (p.Equals("/index", System.StringComparison.OrdinalIgnoreCase))
            {
                p = "/"; // root
            }
            else
            {
                p = p.Substring(0, p.Length - 6); // drop '/index' -> /es/cli OR /cli
                if (p.Length > 1 && !p.EndsWith('/')) p += '/'; // directory form ends with /
            }
        }

        // Normalize locale prefix rules
        if (locale != defaultLocale)
        {
            // If path already starts with /{locale}/ or equals /{locale}, keep it; else prefix
            if (!(p.Equals($"/{locale}", System.StringComparison.OrdinalIgnoreCase) ||
                  p.Equals($"/{locale}/", System.StringComparison.OrdinalIgnoreCase) ||
                  p.StartsWith($"/{locale}/", System.StringComparison.OrdinalIgnoreCase)))
            {
                if (p == "/") p = $"/{locale}/"; // root in locale
                else p = $"/{locale}" + (p.StartsWith("/") ? p : "/" + p); // /{locale}/... preserving existing leading slash
            }
        }
        else
        {
            // Default locale should not retain a prefixed folder (in case content is authored under locale folder unexpectedly)
            if (p.StartsWith($"/{defaultLocale}/", System.StringComparison.OrdinalIgnoreCase))
            {
                p = p.Substring(defaultLocale.Length + 1); // remove '/en'
                if (!p.StartsWith('/')) p = "/" + p;
            }
            else if (p.Equals($"/{defaultLocale}", System.StringComparison.OrdinalIgnoreCase))
            {
                p = "/"; // default locale root
            }
        }

        // Avoid double slashes (defensive)
        while (p.Contains("//")) p = p.Replace("//", "/");

        return SiteConfig.ApplyBasePath(p);
    }
    
    /// <summary>
    /// Gets the country flag emoji for a locale code.
    /// This can be overridden by derived templates to provide custom locale representations.
    /// </summary>
    /// <param name="locale">The locale code (e.g., "en", "es", "fr")</param>
    protected virtual string GetCountryFlag(string locale)
    {
        // Basic default implementation - customers can override this in their templates
        return locale?.ToLower() switch
        {
            "en" => "🇺🇸",
            "es" => "🇪🇸", 
            _ => "🌐"
        };
    }
    
    /// <summary>
    /// Gets the display name for a locale code.
    /// This can be overridden by derived templates to provide custom locale names.
    /// </summary>
    /// <param name="locale">The locale code (e.g., "en", "es", "fr")</param>
    protected virtual string GetLocaleDisplayName(string locale)
    {
        // Basic default implementation - customers can override this in their templates
        return locale?.ToLower() switch
        {
            "en" => "English",
            "es" => "Español",
            _ => locale?.ToUpper() ?? "Unknown"
        };
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
                Parent = this.Model.Node,
                Locale = this.Model.Node.Locale // propagate locale so menu links localize correctly
            };
            var r = await ContentRenderer.FromMarkdownAsync (node);
            r = await ContentRenderer.ToRazorAsync (r);

            return new RawString (r.Body);
        }
    }
}
