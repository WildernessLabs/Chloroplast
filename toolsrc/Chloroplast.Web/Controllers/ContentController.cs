using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Chloroplast.Web.Controllers
{
    public class ContentController : Controller
    {

        private readonly ILogger<ContentController> _logger;

        public ContentController (ILogger<ContentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Load (string pathInfo)
        {
            string url = pathInfo.ToLower ();

            // check filename, if no extension, add /index.html
            string[] urlParts = url.Split ("/", StringSplitOptions.RemoveEmptyEntries);
            if (!Path.HasExtension (urlParts.Last ()))
            {
                urlParts = urlParts.Union (new[] { "index.html" }).ToArray ();
                url = string.Join ('/', urlParts);
            }

            string domain = "";// get from appsettings
            url = $"{domain}/{url}";

            using (var httpClient = new HttpClient ())
            {
                using (var request = new HttpRequestMessage (HttpMethod.Get, url))
                {
                    Stream contentStream = await httpClient.GetStreamAsync (url);
                    return this.File (contentStream, "text/html");
                }
            }
        }
    }
}
