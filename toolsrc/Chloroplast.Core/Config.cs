using System;
using Microsoft.Extensions.Configuration;

namespace Chloroplast.Core
{
    public static class SiteConfig
    {
        public static IConfigurationRoot Instance { get; set; }
    }
}
