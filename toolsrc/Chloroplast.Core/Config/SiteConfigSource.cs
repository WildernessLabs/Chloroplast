using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;

namespace Chloroplast.Core.Config
{
    public class FrontMatterConfigSource : StreamConfigurationSource
    {
        string yaml;
        public FrontMatterConfigSource (string yamlsource) => this.yaml = yamlsource;

        public override IConfigurationProvider Build (IConfigurationBuilder builder)
        {
            var mem = new MemoryStream ();
            var stringBytes = System.Text.Encoding.UTF8.GetBytes (yaml);
            mem.Write (stringBytes, 0, stringBytes.Length);
            mem.Seek (0, SeekOrigin.Begin);
            this.Stream = mem;
            return new FrontMatterConfigurationProvider (this);
        }
    }

    public class FrontMatterConfigurationProvider : StreamConfigurationProvider
    {
        public FrontMatterConfigurationProvider (FrontMatterConfigSource source) : base (source) { }

        public override void Load (Stream stream)
        {
            var parser = new SiteConfigurationFileParser ();

            Data = parser.Parse (stream);
        }
    }

    public class SiteConfigSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build (IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider ();
            return new SiteConfigurationProvider (this);
        }
    }

    public class SiteConfigurationProvider : FileConfigurationProvider
    {
        public SiteConfigurationProvider (FileConfigurationSource source) : base (source) { }

        public override void Load (Stream stream)
        {
            var parser = new SiteConfigurationFileParser ();

            Data = parser.Parse (stream);
        }
    }

    internal class SiteConfigurationFileParser
    {
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string> (StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string> ();
        private string _currentPath;

        public IDictionary<string, string> Parse (string input)
        {
            using (var stream = new MemoryStream ())
            using (var writer = new StreamWriter (stream))
            {
                writer.Write (input);
                writer.Flush ();
                stream.Position = 0;
                return Parse (stream);
            }
        }

        public IDictionary<string, string> Parse (Stream input)
        {
            return Parse (new StreamReader (input, detectEncodingFromByteOrderMarks: true));
        }

        public IDictionary<string, string> Parse (StreamReader input)
        {
            _data.Clear ();
            _context.Clear ();

            // https://dotnetfiddle.net/rrR2Bb
            var yaml = new YamlStream ();
            yaml.Load (input);

            if (yaml.Documents.Any ())
            {
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                // The document node is a mapping node
                VisitYamlMappingNode (mapping);
            }

            return _data;
        }

        private void VisitYamlNodePair (KeyValuePair<YamlNode, YamlNode> yamlNodePair)
        {
            var context = ((YamlScalarNode)yamlNodePair.Key).Value;
            VisitYamlNode (context, yamlNodePair.Value);
        }

        private void VisitYamlNode (string context, YamlNode node)
        {
            if (node is YamlScalarNode scalarNode)
            {
                VisitYamlScalarNode (context, scalarNode);
            }
            if (node is YamlMappingNode mappingNode)
            {
                VisitYamlMappingNode (context, mappingNode);
            }
            if (node is YamlSequenceNode sequenceNode)
            {
                VisitYamlSequenceNode (context, sequenceNode);
            }
        }

        private void VisitYamlScalarNode (string context, YamlScalarNode yamlValue)
        {
            //a node with a single 1-1 mapping
            EnterContext (context);
            var currentKey = _currentPath;

            if (_data.ContainsKey (currentKey))
            {
                throw new FormatException ($"duplicate key: {currentKey}");
            }

            _data[currentKey] = IsNullValue (yamlValue) ? null : yamlValue.Value;
            ExitContext ();
        }

        private void VisitYamlMappingNode (YamlMappingNode node)
        {
            foreach (var yamlNodePair in node.Children)
            {
                VisitYamlNodePair (yamlNodePair);
            }
        }

        private void VisitYamlMappingNode (string context, YamlMappingNode yamlValue)
        {
            //a node with an associated sub-document
            EnterContext (context);

            VisitYamlMappingNode (yamlValue);

            ExitContext ();
        }

        private void VisitYamlSequenceNode (string context, YamlSequenceNode yamlValue)
        {
            //a node with an associated list
            EnterContext (context);

            VisitYamlSequenceNode (yamlValue);

            ExitContext ();
        }

        private void VisitYamlSequenceNode (YamlSequenceNode node)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                VisitYamlNode (i.ToString (), node.Children[i]);
            }
        }

        private void EnterContext (string context)
        {
            _context.Push (context);
            _currentPath = ConfigurationPath.Combine (_context.Reverse ());
        }

        private void ExitContext ()
        {
            _context.Pop ();
            _currentPath = ConfigurationPath.Combine (_context.Reverse ());
        }

        private bool IsNullValue (YamlScalarNode yamlValue)
        {
            return yamlValue.Style == YamlDotNet.Core.ScalarStyle.Plain
                && (
                    yamlValue.Value == "~"
                    || yamlValue.Value == "null"
                    || yamlValue.Value == "Null"
                    || yamlValue.Value == "NULL"
                );
        }
    }
}
