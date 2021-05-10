﻿using System;
using System.Linq;
using EcmaXml = Choroplast.Core.Loaders.EcmaXml;

namespace Choroplast.Core.Loaders.EcmaXml
{
    public partial class Namespace
    {
        public string Summary
        {
            get {
                var sum = (EcmaXml.summary)this.Docs.Items.FirstOrDefault (i => i is EcmaXml.summary);
                return sum == null ? string.Empty : sum.Text.FirstOrDefault ();
            }
        }
    }
}
