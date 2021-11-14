using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EcmaXml = Chloroplast.Core.Loaders.EcmaXml;

namespace Chloroplast.Core.Loaders.EcmaXml
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

    [XmlType("Overview")]
    public class XIndex
    {
        [XmlElement]
        public string Title { get; set; }
        //types
        [XmlArray ("Types")]
        [XmlArrayItem ("Namespace", Type = typeof (XIndexNamespace))]
        public List<XIndexNamespace> Namespaces { get; set; }
        //assemblies
        //remarks
        //copyright
        //extension methods

        public string ToMenu(string rootPath)
        {
            StringBuilder sb = new StringBuilder ();
            sb.AppendLine (@"---
template: menu
parent:
- path: "+ rootPath +@"
  title: Home
navTree:");
            foreach(var ns in this.Namespaces)
            {
                sb.AppendLine (@"- path: "+ rootPath +@"/"+ ns.Name +@"
  title: "+ ns.Name +@"");
                if (ns.Types.Any ())
                {
                    sb.AppendLine ("  items: ");
                    foreach (var t in ns.Types)
                    {
                        sb.AppendLine ($"  - path: {rootPath}/{ns.Name}/{t.Name}.html");
                        sb.AppendLine ($"    title: {t.Name}");
                    }
                }
            }

            sb.AppendLine ("---");

            return sb.ToString ();
        }
    }

    [XmlType("Namespace")]
    public class XIndexNamespace
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Type")]
        public List<XIndexType> Types { get; set; }
    }

    [XmlType("Type")]
    public class XIndexType
    {
        [XmlAttribute]
        public string Name { get; set; }

        private string displayName;

        [XmlAttribute]
        public string DisplayName
        {
            get
            {
                return string.IsNullOrWhiteSpace (this.displayName) ? this.Name : this.displayName;
            }
            set
            {
                this.displayName = value;
            }
        }

        [XmlAttribute]
        public string Kind { get; set; }
    }

    [Serializable]
    [XmlType("Type")]
    public class XType
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string FullName { get; set; }

        public string Namespace {
            get
            {
                return FullName.Replace ("." + Name, string.Empty);
            }
        }

        [XmlElement ("TypeSignature")]
        public List<XSignature> Signatures { get; set; } = new List<XSignature> ();

        [XmlElement ("AssemblyInfo")]
        public List<XAssemblyInfo> AssemblyInfos { get; set; } = new List<XAssemblyInfo> ();

        [XmlArray ("Members")]
        [XmlArrayItem ("Member", Type = typeof (XMemberItem))]
        public XMemberItem[] Members { get; set; } = new XMemberItem[0];

        public XBase Base { get; set; }

        [XmlArray ("Parameters")]
        [XmlArrayItem ("Parameter", Type = typeof (XParameter))]
        public List<XParameter> Parameters { get; set; } = new List<XParameter> ();
        [XmlArray ("TypeParameters")]
        [XmlArrayItem ("TypeParameter", Type = typeof (XParameter))]
        public List<XParameter> TypeParameters { get; set; } = new List<XParameter> ();

        public XDocs Docs { get; set; }

        // class type
        // interfaces
        // attributes
    }

    [XmlType("Docs")]
    public class XDocs
    {
        [XmlElement ("summary")]
        public string Summary { get; set; } = string.Empty;
        [XmlElement ("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [XmlElement ("param")]
        public List<XParam> Params { get; set; } = new List<XParam> ();
        [XmlElement ("typeparam")]
        public List<XParam> TypeParams { get; set; } = new List<XParam> ();
    }

    public class XParam
    {
        [XmlAttribute ("name")]
        public string Name { get; set; } = string.Empty;

        [XmlText()]
        public string Value { get; set; } = string.Empty;
    }

    public class XParameter
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public string RefType { get; set; }
        // attributes
        // default value
    }

    [XmlType ("Member")]
    public class XMemberItem
    {
        [XmlAttribute("MemberName")]
        public string Name { get; set; } 

        public string FullName { get; set; }

        public string MemberType { get; set; }

        [XmlElement ("MemberSignature")]
        public List<XSignature> Signatures { get; set; } = new List<XSignature> ();

        [XmlElement ("AssemblyInfo")]
        public List<XAssemblyInfo> AssemblyInfos { get; set; } = new List<XAssemblyInfo> ();

        [XmlArray ("Parameters")]
        [XmlArrayItem ("Parameter", Type = typeof (XParameter))]
        public List<XParameter> Parameters { get; set; } = new List<XParameter> ();
        [XmlArray ("TypeParameters")]
        [XmlArrayItem ("TypeParameter", Type = typeof (XParameter))]
        public List<XParameter> TypeParameters { get; set; } = new List<XParameter> ();

        public XDocs Docs { get; set; }

        // parameter attributes
    }

    //[XmlType ("MemberSignature")]
    //[XmlType ("TypeSignature")]
    public class XSignature
    {
        [XmlAttribute]
        public string Language { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

    }

    [XmlType("AssemblyInfo")]
    public class XAssemblyInfo
    {
        [XmlElement("AssemblyVersion")]
        public List<string> AssemblyVersion { get; set; }
        public string AssemblyName { get; set; }
    }

    [XmlType("Base")]
    public class XBase
    {
        public string BaseTypeName { get; set; }

        // TODO: BaseTypeArguments
        // BaseTypeArgument TypeParamName="U">T</
    }
}
