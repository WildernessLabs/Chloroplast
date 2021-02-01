using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chloroplast.Web.Controllers
{
    public class ContentController : Controller
    {

        private readonly ILogger<ContentController> _logger;
        private readonly IConfiguration config;
        

        public ContentController (IConfiguration config, ILogger<ContentController> logger)
        {
            _logger = logger;
            this.config = config;
        }

        [HttpGet]
        public async Task<ActionResult> Load (string pathInfo)
        {
            string url = pathInfo?.ToLower () ?? string.Empty;

            // check filename, if no extension, add /index.html
            string[] urlParts = url.Split ("/", StringSplitOptions.RemoveEmptyEntries);
            if (!Path.HasExtension (urlParts.LastOrDefault () ?? string.Empty))
            {
                urlParts = urlParts.Union (new[] { "index.html" }).ToArray ();
                url = string.Join ('/', urlParts);
            }

            string domain = this.config["domainRoot"];// get from appsettings
            if (string.IsNullOrWhiteSpace (domain))
            {
                return Content ("please add 'domainRoot' to your appsettings", "text/html");
            }

            url = $"{domain}/{url}";

            using (var httpClient = new HttpClient ())
            {
                using (var request = new HttpRequestMessage (HttpMethod.Get, url))
                {
                    try
                    {
                        string x10 = Path.GetExtension (url);
                        string mimetype;
                        if (!MimeTypes.MimeTypeMap.TryGetMimeType (x10, out mimetype))
                            mimetype = $"application/{x10}";

                        //Stream contentStream = await httpClient.GetStreamAsync (url);

                        var response = await httpClient.GetAsync (url);
                        if (response.IsSuccessStatusCode)
                            return this.File (await response.Content.ReadAsStreamAsync(), mimetype);
                        else
                        {
                            this.Response.StatusCode = (int)response.StatusCode;
                            
                            return this.Content (response.ReasonPhrase, "text/plain");
                        }
                    }
                    catch (HttpRequestException hrex)
                    {
                        this.Response.StatusCode = 404;

                        return this.Content (hrex.ToString(), "text/plain");
                    }
                }
            }
        }
    }
}
