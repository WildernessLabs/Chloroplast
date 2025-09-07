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
        public string MenuPath { get; set; }
        
        /// <summary>
        /// The locale of this content node (e.g., "en", "es", "fr").
        /// </summary>
        public string Locale { get; set; }
        
        /// <summary>
        /// Indicates whether this content was machine translated.
        /// </summary>
        public bool IsMachineTranslated { get; set; }
        
        /// <summary>
        /// Array of translated versions of this content node.
        /// The default language version is not included in this array.
        /// </summary>
        public ContentNode[] Translations { get; set; } = new ContentNode[0];

    /// <summary>
    /// True when this node is a synthesized fallback for a locale that doesn't yet
    /// have an authored translation. Content is sourced from the default locale.
    /// </summary>
    public bool IsFallback { get; set; }

        public ContentNode ()
        {
        }

        public ContentNode Parent { get; set; }
        public IList<ContentNode> Children { get; } = new List<ContentNode> ();

        public override string ToString ()
        {
            return $"{Slug}, {Title}, {Source}->{Target} ({Locale}{(IsFallback ? ", fallback" : string.Empty)})";
        }
    }
}
