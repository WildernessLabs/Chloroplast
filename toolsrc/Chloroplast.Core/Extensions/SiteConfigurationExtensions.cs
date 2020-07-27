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
    }
}
