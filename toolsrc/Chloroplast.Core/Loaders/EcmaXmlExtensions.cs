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

        [XmlArray ("Members")]
        [XmlArrayItem ("Member", Type = typeof (XMemberItem))]
        public XMemberItem[] Members { get; set; } = new XMemberItem[0];
    }

    [XmlType ("Member")]
    public class XMemberItem
    {
        [XmlAttribute("MemberName")]
        public string Name { get; set; } 

        public string FullName { get; set; }

        public string MemberType { get; set; }

        [XmlElement ("MemberSignature")]
        public List<XSignature> Signatures { get; set; }

        [XmlElement ("AssemblyInfo")]
        public List<XAssemblyInfo> AssemblyInfos { get; set; }

        // base
        // interfaces
        // assemblyinfo
        // typeparameters
        // docs
    }

    [XmlType ("MemberSignature")]
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
        public string AssemblyVersion { get; set; }
        public string AssemblyName { get; set; }
    }
}
