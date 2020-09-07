using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Chloroplast.Core;
using Chloroplast.Core.Content;
using Chloroplast.Core.Extensions;
using Xunit;

namespace Chloroplast.Test
{
    public class TreeTests
    {
        [Fact]
        public void TopLevels ()
        {
            List<ContentNode> nodes = new List<ContentNode> ();
            nodes.Add (MakeNode ("one"));
            nodes.Add (MakeNode ($"one".CombinePath ("two")));
            nodes.Add (MakeNode ($"one".CombinePath ("three")));
            nodes.Add (MakeNode ($"one".CombinePath ("four")));

            ContentArea area = new ContentArea (nodes);
            var result = area.BuildHierarchy ();

            Assert.True (result.First ().Children.Count == 3);
        }

        [Fact]
        public void MultipleLevels ()
        {
            List<ContentNode> nodes = new List<ContentNode> ();
            nodes.Add (MakeNode ("one"));
            nodes.Add (MakeNode ($"one".CombinePath ("two")));
            nodes.Add (MakeNode ($"one".CombinePath ("two").CombinePath("one")));
            nodes.Add (MakeNode ($"one".CombinePath ("three")));
            nodes.Add (MakeNode ($"one".CombinePath ("three").CombinePath ("one")));
            nodes.Add (MakeNode ($"one".CombinePath ("four")));
            nodes.Add (MakeNode ($"one".CombinePath ("four").CombinePath ("one")));
            nodes.Add (MakeNode ($"two"));

            ContentArea area = new ContentArea (nodes);
            var result = area.BuildHierarchy ().ToArray();

            Assert.True (result[0].Children.Count == 3); //one/*
            Assert.True (result[1].Children.Count == 0); //two/*
            Assert.True (result[0].Children[0].Children.Count == 1); // one/two/*
            Assert.True (result[0].Children[1].Children.Count == 1); // one/three/*
            Assert.True (result[0].Children[2].Children.Count == 1); // one/four/*


        }

        private static ContentNode MakeNode (string path)
        {
            return new ContentNode
            {
                Slug = "one",
                Title = path  + " title",
                Source = new TestSource (path)
            };
        }
    }

    class TestSource : IFile
    {
        public DateTime LastUpdated { get; set; }
        public string RootRelativePath { get; set; }

        public TestSource(string rootPath)
        {
            this.RootRelativePath = rootPath;
            LastUpdated = DateTime.Now;
        }

        public void CopyTo (IFile target)
        {
            throw new NotImplementedException ();
        }

        public Task<string> ReadContentAsync ()
        {
            throw new NotImplementedException ();
        }

        public Task WriteContentAsync (string content)
        {
            throw new NotImplementedException ();
        }

        public override string ToString ()
        {
            return this.RootRelativePath;
        }
    }
}
