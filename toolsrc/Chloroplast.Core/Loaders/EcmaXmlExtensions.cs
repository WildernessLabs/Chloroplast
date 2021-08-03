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

        // Type parameters
        // interfaces
        // docs
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

        // generic parameters
        // parameters
        // docs
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
    }
}
