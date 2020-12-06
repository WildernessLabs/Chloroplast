using System;
using System.Collections.Generic;
using Chloroplast.Core.Content;

namespace Chloroplast.Core
{
    public class MenuNode
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public IEnumerable<MenuNode> Items { get; set; }
    }

    public class ContentNode
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public IFile Source { get; set; }
        public IFile Target { get; set; }
        public ContentArea Area { get; set; }

        public ContentNode ()
        {
        }

        public ContentNode Parent { get; set; }
        public IList<ContentNode> Children { get; } = new List<ContentNode> ();

        public override string ToString ()
        {
            return $"{Slug}, {Title}, {Source}->{Target}";
        }
    }
}
