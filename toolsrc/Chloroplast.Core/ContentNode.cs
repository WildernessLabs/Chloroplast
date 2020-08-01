﻿using System;
using System.Collections.Generic;
using Chloroplast.Core.Content;

namespace Chloroplast.Core
{
    public class ContentNode
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public IFile Source { get; set; }
        public IFile Target { get; set; }

        public ContentNode ()
        {
        }

        public IList<ContentNode> Children { get; } = new List<ContentNode> ();
    }
}