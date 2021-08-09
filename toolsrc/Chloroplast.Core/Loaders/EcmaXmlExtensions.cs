using System;
using System.Collections.Generic;
using System.Linq;
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

    [Serializable]
    [XmlType("Type")]
    public class XType
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string FullName { get; set; }

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
