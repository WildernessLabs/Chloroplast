using System;
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


    [XmlType ("Members")]
    public class XMembers
    {
        [XmlElement ("Members")]
        public XMemberItem[] Members { get; set; }
    }

    [XmlType ("Member")]
    public class XMemberItem
    {
        [XmlAttribute("MemberName")]
        public string Name { get; set; } 

    }
}
