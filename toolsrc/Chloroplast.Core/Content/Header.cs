using System;
using HtmlAgilityPack;

namespace Chloroplast.Core.Content
{
    public class Header
    {
        public int Level { get; set; }
        public string Value { get; set; }
        public string Slug
        {
            get
            {
                return this.Value
                    .Replace (" ", "_")
                    .Replace ("!", "")
                    .Replace ("?", "")
                    .Replace ("#", "_")
                    .Replace (".", "_")
                    .Replace ("<", "_")
                    .Replace (">", "_")
                    .Replace ("\"", "_")
                    .Replace("'", "_")
                    .Replace ("&amp;", "and");
            }
        }

        internal static Header FromNode (HtmlNode n)
        {
            return new Header
            {
                Value = n.InnerText,
                Level = GetLevel (n)
            };
        }

        private static int GetLevel (HtmlNode n)
        {
            string tagName = n.Name.ToLower ();
            switch (tagName)
            {
                case "h1":
                    return 1;
                case "h2":
                    return 2;
                case "h3":
                    return 3;
                case "h4":
                    return 4;
                case "h5":
                    return 5;
                case "h6":
                    return 6;
                default:
                    return 1;
            }
        }
    }
}
