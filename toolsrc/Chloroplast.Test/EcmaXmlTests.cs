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
        public void TestLoadNamespace()
        {
            var ns = EcmaXmlLoader.LoadNamespace (XmlForNS);

            Assert.Equal ("Meadow.Foundation.Audio", ns.Name);
            Assert.Equal ("To be added.", ns.Summary);
        }

        [Fact]
        public void TestLoadType()
        {
            var t = EcmaXmlLoader.LoadXType (XmlForSimpleType);

            Assert.Equal ("Test", t.Name);
            Assert.Single (t.Members);
            Assert.Equal (".ctor", t.Members.First ().Name);
            Assert.Equal (2, t.Members.First ().Signatures.Count);
            Assert.Equal ("protected App ();", t.Members.First ().Signatures.First ().Value);
            Assert.Equal ("Constructor", t.Members.First ().MemberType);
            Assert.Single (t.Members.First ().AssemblyInfos);
            Assert.Equal ("0.22.0.0", t.Members.First ().AssemblyInfos.First().AssemblyVersion);
        }



        private static string XmlForNS = @"<Namespace Name=""Meadow.Foundation.Audio"">
  <Docs>
    <summary>To be added.</summary>
    <remarks>To be added.</remarks>
  </Docs>
</Namespace>
";
        private static string XmlForSimpleType = @"<Type Name=""Test"">
  <Members>
    <Member MemberName="".ctor"">
      <MemberSignature Language=""C#"" Value=""protected App ();"" />
      <MemberSignature Language=""ILAsm"" Value="".method familyhidebysig specialname rtspecialname instance void .ctor() cil managed"" />
      <MemberType>Constructor</MemberType>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <Parameters />
      <Docs>
        <summary>To be added.</summary>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
  </Members>

</Type>";

        private static string XmlForGenericType = @"<Type Name=""App&lt;D,A&gt;"" FullName=""Meadow.App&lt;D,A&gt;"">
  <TypeSignature Language=""C#"" Value=""public abstract class App&lt;D,A&gt; : Meadow.IApp where D : class, IIODevice where A : class, IApp"" />
  <TypeSignature Language=""ILAsm"" Value="".class public auto ansi abstract App`2&lt;class (class Meadow.Hardware.IIODevice) D, class (class Meadow.IApp) A&gt; extends System.Object implements class Meadow.IApp"" />
  <AssemblyInfo>
    <AssemblyName>Meadow</AssemblyName>
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
  <Members>
    <Member MemberName="".ctor"">
      <MemberSignature Language=""C#"" Value=""protected App ();"" />
      <MemberSignature Language=""ILAsm"" Value="".method familyhidebysig specialname rtspecialname instance void .ctor() cil managed"" />
      <MemberType>Constructor</MemberType>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <Parameters />
      <Docs>
        <summary>To be added.</summary>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
    <Member MemberName=""Current"">
      <MemberSignature Language=""C#"" Value=""public static A Current { get; }"" />
      <MemberSignature Language=""ILAsm"" Value="".property !A Current"" />
      <MemberType>Property</MemberType>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <ReturnValue>
        <ReturnType>A</ReturnType>
      </ReturnValue>
      <Docs>
        <summary>To be added.</summary>
        <value>To be added.</value>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
    <Member MemberName=""Device"">
      <MemberSignature Language=""C#"" Value=""public static D Device { get; }"" />
      <MemberSignature Language=""ILAsm"" Value="".property !D Device"" />
      <MemberType>Property</MemberType>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <ReturnValue>
        <ReturnType>D</ReturnType>
      </ReturnValue>
      <Docs>
        <summary>To be added.</summary>
        <value>To be added.</value>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
    <Member MemberName=""OnWake"">
      <MemberSignature Language=""C#"" Value=""public virtual void OnWake ();"" />
      <MemberSignature Language=""ILAsm"" Value="".method public hidebysig newslot virtual instance void OnWake() cil managed"" />
      <MemberType>Method</MemberType>
      <Implements>
        <InterfaceMember>M:Meadow.IApp.OnWake</InterfaceMember>
      </Implements>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <ReturnValue>
        <ReturnType>System.Void</ReturnType>
      </ReturnValue>
      <Parameters />
      <Docs>
        <summary>To be added.</summary>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
    <Member MemberName=""WillReset"">
      <MemberSignature Language=""C#"" Value=""public virtual void WillReset ();"" />
      <MemberSignature Language=""ILAsm"" Value="".method public hidebysig newslot virtual instance void WillReset() cil managed"" />
      <MemberType>Method</MemberType>
      <Implements>
        <InterfaceMember>M:Meadow.IApp.WillReset</InterfaceMember>
      </Implements>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <ReturnValue>
        <ReturnType>System.Void</ReturnType>
      </ReturnValue>
      <Parameters />
      <Docs>
        <summary>To be added.</summary>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
    <Member MemberName=""WillSleep"">
      <MemberSignature Language=""C#"" Value=""public virtual void WillSleep ();"" />
      <MemberSignature Language=""ILAsm"" Value="".method public hidebysig newslot virtual instance void WillSleep() cil managed"" />
      <MemberType>Method</MemberType>
      <Implements>
        <InterfaceMember>M:Meadow.IApp.WillSleep</InterfaceMember>
      </Implements>
      <AssemblyInfo>
        <AssemblyVersion>0.22.0.0</AssemblyVersion>
      </AssemblyInfo>
      <ReturnValue>
        <ReturnType>System.Void</ReturnType>
      </ReturnValue>
      <Parameters />
      <Docs>
        <summary>To be added.</summary>
        <remarks>To be added.</remarks>
      </Docs>
    </Member>
  </Members>
</Type>
";
    }
}
