using System;
using System.Threading.Tasks;
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
    }
}
