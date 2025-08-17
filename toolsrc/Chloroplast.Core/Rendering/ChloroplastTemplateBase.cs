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
    /// Gets the URL for the current page in the specified locale.
    /// If the page doesn't exist in that locale, returns the default language version.
    /// </summary>
    /// <param name="locale">The target locale</param>
    protected string GetLocalizedPageUrl(string locale)
    {
        if (Model?.Node?.Translations != null)
        {
            var translation = System.Array.Find(Model.Node.Translations, t => t.Locale == locale);
            if (translation != null)
            {
                var translatedPath = "/" + translation.Target.RootRelativePath.Replace(".html", "").Replace("\\", "/");
                return LocaleHref(translatedPath, locale);
            }
        }
        
        // Fallback to current page path with locale prefix
        var currentPath = Model?.Node?.Target?.RootRelativePath?.Replace(".html", "").Replace("\\", "/");
        if (!string.IsNullOrEmpty(currentPath))
        {
            currentPath = "/" + currentPath;
            return LocaleHref(currentPath, locale);
        }
        
        return LocaleHref("/", locale);
    }
    
    /// <summary>
    /// Gets the country flag emoji for a locale code.
    /// </summary>
    /// <param name="locale">The locale code (e.g., "en", "es", "fr")</param>
    protected string GetCountryFlag(string locale)
    {
        return locale?.ToLower() switch
        {
            "en" => "🇺🇸", // English - US flag
            "es" => "🇪🇸", // Spanish - Spain flag
            "fr" => "🇫🇷", // French - France flag
            "de" => "🇩🇪", // German - Germany flag
            "it" => "🇮🇹", // Italian - Italy flag
            "pt" => "🇵🇹", // Portuguese - Portugal flag
            "ja" => "🇯🇵", // Japanese - Japan flag
            "ko" => "🇰🇷", // Korean - South Korea flag
            "zh" => "🇨🇳", // Chinese - China flag
            "ru" => "🇷🇺", // Russian - Russia flag
            "ar" => "🇸🇦", // Arabic - Saudi Arabia flag
            "hi" => "🇮🇳", // Hindi - India flag
            "nl" => "🇳🇱", // Dutch - Netherlands flag
            "sv" => "🇸🇪", // Swedish - Sweden flag
            "da" => "🇩🇰", // Danish - Denmark flag
            "no" => "🇳🇴", // Norwegian - Norway flag
            "fi" => "🇫🇮", // Finnish - Finland flag
            "pl" => "🇵🇱", // Polish - Poland flag
            "tr" => "🇹🇷", // Turkish - Turkey flag
            "cs" => "🇨🇿", // Czech - Czech Republic flag
            "hu" => "🇭🇺", // Hungarian - Hungary flag
            "ro" => "🇷🇴", // Romanian - Romania flag
            "bg" => "🇧🇬", // Bulgarian - Bulgaria flag
            "hr" => "🇭🇷", // Croatian - Croatia flag
            "sk" => "🇸🇰", // Slovak - Slovakia flag
            "sl" => "🇸🇮", // Slovenian - Slovenia flag
            "et" => "🇪🇪", // Estonian - Estonia flag
            "lv" => "🇱🇻", // Latvian - Latvia flag
            "lt" => "🇱🇹", // Lithuanian - Lithuania flag
            "mt" => "🇲🇹", // Maltese - Malta flag
            "cy" => "🏴󠁧󠁢󠁷󠁬󠁳󠁿", // Welsh - Wales flag
            "ga" => "🇮🇪", // Irish - Ireland flag
            "eu" => "🏴", // Basque - generic flag
            "ca" => "🏴󠁥󠁳󠁣󠁴󠁿", // Catalan - Catalonia flag
            _ => "🌐" // Generic globe for unknown locales
        };
    }
    
    /// <summary>
    /// Gets the display name for a locale code.
    /// </summary>
    /// <param name="locale">The locale code (e.g., "en", "es", "fr")</param>
    protected string GetLocaleDisplayName(string locale)
    {
        return locale?.ToLower() switch
        {
            "en" => "English",
            "es" => "Español",
            "fr" => "Français",
            "de" => "Deutsch",
            "it" => "Italiano",
            "pt" => "Português",
            "ja" => "日本語",
            "ko" => "한국어",
            "zh" => "中文",
            "ru" => "Русский",
            "ar" => "العربية",
            "hi" => "हिन्दी",
            "nl" => "Nederlands",
            "sv" => "Svenska",
            "da" => "Dansk",
            "no" => "Norsk",
            "fi" => "Suomi",
            "pl" => "Polski",
            "tr" => "Türkçe",
            "cs" => "Čeština",
            "hu" => "Magyar",
            "ro" => "Română",
            "bg" => "Български",
            "hr" => "Hrvatski",
            "sk" => "Slovenčina",
            "sl" => "Slovenščina",
            "et" => "Eesti",
            "lv" => "Latviešu",
            "lt" => "Lietuvių",
            "mt" => "Malti",
            "cy" => "Cymraeg",
            "ga" => "Gaeilge",
            "eu" => "Euskera",
            "ca" => "Català",
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
                Parent = this.Model.Node
            };
            var r = await ContentRenderer.FromMarkdownAsync (node);
            r = await ContentRenderer.ToRazorAsync (r);

            return new RawString (r.Body);
        }
    }
}
