using System;
using System.Threading.Tasks;

namespace Chloroplast.Core.Content
{
    public interface IFile
    {
        DateTime LastUpdated { get; set; }
        string RootRelativePath { get; set; }
        void CopyTo (IFile target);
        Task<string> ReadContentAsync ();
        Task WriteContentAsync (string content);
    }
}
