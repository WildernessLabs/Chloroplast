using System;
using System.IO;
using System.Threading.Tasks;
using Chloroplast.Core.Content;
using Chloroplast.Core.Extensions;
using MiniRazor.Primitives;

namespace Chloroplast.Core.Rendering
{
    public abstract class ChloroplastTemplateBase<T> : MiniRazor.MiniRazorTemplateBase<T>
    {
        public ChloroplastTemplateBase ()
        {
        }

        protected Task<RawString> PartialAsync<K>(string templateName, K model)
        {
            return RazorRenderer.Instance.RenderTemplateContent (templateName, model);
        }

        protected async Task<RawString> PartialAsync(string menuPath)
        {
            string fullMenuPath = SiteConfig.Instance["root"]
                .NormalizePath ()
                .CombinePath (menuPath);

               // load the menu path
               var node = new ContentNode
            {
                Slug = menuPath,
                Source = new DiskFile (fullMenuPath, menuPath),
                Target = new DiskFile (fullMenuPath, menuPath) // not used
            };
            var r = await ContentRenderer.FromMarkdownAsync (node);
            r = await ContentRenderer.ToRazorAsync (r);

            return new RawString (r.Body);
        }
    }
}
