using System;
using System.IO;
using System.Linq;
using Chloroplast.Core.Loaders;
using Chloroplast.Core.Loaders.EcmaXml;
using Xunit;
using EcmaXml = Chloroplast.Core.Loaders.EcmaXml;

namespace Chloroplast.Test
{
    public class EcmaXmlTests
    {
        [Fact]
        public void TestLoadIndex()
        {
            var index = EcmaXmlLoader.LoadXIndex (XmlForIndex);

            Assert.Single (index.Namespaces);
            Assert.Equal ("Meadow", index.Title);
            var ns = index.Namespaces.Single ();
            Assert.Equal ("Meadow", ns.Name);

            Assert.Equal (5, ns.Types.Count);

            Assert.Equal ("AnalogCapabilities", ns.Types[0].Name);
            Assert.Equal ("AnalogCapabilities", ns.Types[0].DisplayName);
            Assert.Equal ("Class", ns.Types[0].Kind);
        }

        [Fact]
        public void TestLoadNamespace()
        {
            var ns = EcmaXmlLoader.LoadNamespace (XmlForNS);

            Assert.Equal ("Meadow.Foundation.Audio", ns.Name);
            Assert.Equal ("To be added.", ns.Summary);
        }

        [Fact]
        public void TestLoadType ()
        {
            var t = EcmaXmlLoader.LoadXType (XmlForTypeOnlyDetails);

            Assert.Equal ("App<D,A>", t.Name);
            Assert.Equal ("Meadow.App<D,A>", t.FullName);
            Assert.Equal (2, t.Signatures.Count);
            Assert.Single (t.AssemblyInfos);
            Assert.Equal (2, t.AssemblyInfos.First ().AssemblyVersion.Count);
            Assert.Equal ("0.21.0.0", t.AssemblyInfos.First ().AssemblyVersion.First());
            Assert.Equal ("System.Object", t.Base.BaseTypeName);

            // docs
            var docs = t.Docs;
            Assert.NotNull (docs);
            Assert.Equal ("To be added.", docs.Summary);
            Assert.Equal ("To be added.", docs.Remarks);
            Assert.Equal (2, docs.TypeParams.Count);
            Assert.Equal ("D", docs.TypeParams.First ().Name);
            Assert.Equal ("To be added.", docs.TypeParams.First ().Value);
        }

        [Fact]
        public void TestLoadTypeMember()
        {
            var t = EcmaXmlLoader.LoadXType (XmlForMembers);

            Assert.Equal ("Test", t.Name);
            Assert.Single (t.Members);
            Assert.Equal (".ctor", t.Members.First ().Name);
            Assert.Equal (2, t.Members.First ().Signatures.Count);
            Assert.Equal ("protected App ();", t.Members.First ().Signatures.First ().Value);
            Assert.Equal ("Constructor", t.Members.First ().MemberType);
            Assert.Single (t.Members.First ().AssemblyInfos);
            Assert.Equal ("0.22.0.0", t.Members.First ().AssemblyInfos.First().AssemblyVersion.First());

            // parameter lists
            var member = t.Members.First ();
            
            Assert.Equal (4, member.Parameters.Count);
            Assert.Equal ("a", member.Parameters[0].Name);
            Assert.Equal ("System.String", member.Parameters[1].Type);

            Assert.Single (member.TypeParameters);

            // docs
            var docs = t.Members.First().Docs;
            Assert.NotNull (docs);
            Assert.Equal ("To be added.", docs.Summary);
            Assert.Equal ("To be added.", docs.Remarks);
            Assert.Equal (4, docs.Params.Count);
            Assert.Equal ("a", docs.Params.First ().Name);
            Assert.Equal ("To be added.", docs.Params.First ().Value);
        }


        private static string XmlForIndex = @"<Overview>
  <Assemblies>
    <Assembly Name=""Meadow"" Version=""0.25.0.0"">
      <Attributes>
        <Attribute>
          <AttributeName>System.Diagnostics.Debuggable(System.Diagnostics.DebuggableAttribute+DebuggingModes.IgnoreSymbolStoreSequencePoints)</AttributeName>
        </Attribute>
      </Attributes>
    </Assembly>
  </Assemblies>
  <Remarks>To be added.</Remarks>
  <Copyright>To be added.</Copyright>
  <Types>
    <Namespace Name=""Meadow"">
      <Type Name=""AnalogCapabilities"" Kind=""Class"" />
      <Type Name=""ByteOrder"" Kind=""Enumeration"" />
      <Type Name=""ChangeResult`1"" DisplayName=""ChangeResult&lt;UNIT&gt;"" Kind=""Structure"" />
      <Type Name=""IChangeResult`1"" DisplayName=""IChangeResult&lt;UNIT&gt;"" Kind=""Interface"" />
      <Type Name=""IF7MeadowDevice"" Kind=""Interface"" />
    </Namespace>
  </Types>
  <Title>Meadow</Title>
  <ExtensionMethods>
  </ExtensionMethods>
</Overview>
";
        private static string XmlForNS = @"<Namespace Name=""Meadow.Foundation.Audio"">
  <Docs>
    <summary>To be added.</summary>
    <remarks>To be added.</remarks>
  </Docs>
</Namespace>
";
        private static string XmlForTypeOnlyDetails = @"<Type Name=""App&lt;D,A&gt;"" FullName=""Meadow.App&lt;D,A&gt;"">
  <TypeSignature Language=""C#"" Value=""public abstract class App&lt;D,A&gt; : Meadow.IApp where D : class, IIODevice where A : class, IApp"" />
  <TypeSignature Language=""ILAsm"" Value="".class public auto ansi abstract App`2&lt;class (class Meadow.Hardware.IIODevice) D, class (class Meadow.IApp) A&gt; extends System.Object implements class Meadow.IApp"" />
  <AssemblyInfo>
    <AssemblyName>Meadow</AssemblyName>
    <AssemblyVersion>0.21.0.0</AssemblyVersion>
    <AssemblyVersion>0.22.0.0</AssemblyVersion>
  </AssemblyInfo>
  <TypeParameters>
    <TypeParameter Name=""D"">
      <Constraints>
        <ParameterAttribute>ReferenceTypeConstraint</ParameterAttribute>
        <InterfaceName>Meadow.Hardware.IIODevice</InterfaceName>
      </Constraints>
    </TypeParameter>
    <TypeParameter Name=""A"">
      <Constraints>
        <ParameterAttribute>ReferenceTypeConstraint</ParameterAttribute>
        <InterfaceName>Meadow.IApp</InterfaceName>
      </Constraints>
    </TypeParameter>
  </TypeParameters>
  <Base>
    <BaseTypeName>System.Object</BaseTypeName>
  </Base>
  <Interfaces>
    <Interface>
      <InterfaceName>Meadow.IApp</InterfaceName>
    </Interface>
  </Interfaces>
  <Docs>
    <typeparam name=""D"">To be added.</typeparam>
    <typeparam name=""A"">To be added.</typeparam>
    <summary>To be added.</summary>
    <remarks>To be added.</remarks>
  </Docs>
</Type>";

        private static string XmlForMembers = @"<Type Name=""Test"">
  <Members>
    <Member MemberName="".ctor"">
      <MemberSignature Language=""C#"" Value=""protected App ();"" />
      <MemberSignature Language=""ILAsm"" Value="".method familyhidebysig specialname rtspecialname instance void .ctor() cil managed"" />
      <MemberType>Constructor</MemberType>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <TypeParameters>
        <TypeParameter Name=""T"">
          <Attributes>
            <Attribute>
              <AttributeName Language=""C#"">[Mono.DocTest.Doc (""Type Parameter!"")]</AttributeName>
            </Attribute>
          </Attributes>
        </TypeParameter>
      </TypeParameters>
      <Parameters>
        <Parameter Name=""a"" Type=""System.Int32"" RefType=""ref"" Index=""0"" />
        <Parameter Name = ""b"" Type=""System.String"" Index=""1"" FrameworkAlternate=""One;Three"" />
        <Parameter Name = ""d"" Type=""System.String"" Index=""1"" FrameworkAlternate=""Two"" />
        <Parameter Name = ""c"" Type=""System.Int32"" Index=""2"" />
      </Parameters>
      <Docs>
        <param name=""a"">To be added.</param>
        <param name=""b"">To be added.</param>
        <param name=""d"">To be added.</param>
        <param name=""c"">To be added.</param>
        <summary>To be added.</summary>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
  </Members>

</Type>";

    }
}
