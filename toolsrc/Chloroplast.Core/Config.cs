using System;
using Chloroplast.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Core
{
    public static class SiteConfig
    {
        public static IConfigurationRoot Instance { get; set; }
        public static string BuildVersion { get; set; }
        public static bool CacheBustingEnabled => Instance?.GetBool("cacheBusting:enabled", defaultValue: true) ?? true;
    }
}
