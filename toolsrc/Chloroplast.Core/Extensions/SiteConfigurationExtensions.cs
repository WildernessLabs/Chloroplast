using System.IO;
using Chloroplast.Core.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Chloroplast.Core.Extensions
{
    public static class SiteConfigurationExtensions
    {
        public static IConfigurationBuilder AddChloroplastConfig (this IConfigurationBuilder builder, string path)
        {
            return AddChloroplastConfig (builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddChloroplastConfig (this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddChloroplastConfig (builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddChloroplastConfig (this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddChloroplastConfig (builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddChloroplastConfig (this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            if (provider == null && Path.IsPathRooted (path))
            {
                provider = new PhysicalFileProvider (Path.GetDirectoryName (path));
                path = Path.GetFileName (path);
            }
            var source = new SiteConfigSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add (source);
            return builder;
        }

        public static IConfigurationBuilder AddChloroplastFrontMatter(this IConfigurationBuilder builder, string content)
        {
            var source = new FrontMatterConfigSource (content);
            builder.Add (source);
            return builder;
        }

        public static bool ContainsKey(this IConfigurationRoot config, string key)
        {
            var value = config[key];
            bool hasStringKey = !string.IsNullOrWhiteSpace (value);

            if (!hasStringKey)
            {
                var section = config.GetSection (key);
                return section.Exists();
            }

            return true;
        }

        public static bool ContainsKey (this IConfigurationSection config, string key)
        {
            var value = config[key];
            bool hasStringKey = !string.IsNullOrWhiteSpace (value);

            if (!hasStringKey)
            {
                var section = config.GetSection (key);
                return section != null;
            }

            return true;
        }
    }
}
