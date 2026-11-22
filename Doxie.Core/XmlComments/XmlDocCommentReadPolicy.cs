// ----------------------------------------------------------------------------
// Based on Jolt.NET's implementation
// Original Code: https://jolt.codeplex.com
// RedGate's Fork on GitHub: https://github.com/red-gate/JoltNet-core

/*
License:  New BSD License
Copyright (c) 2007, Steve Guidi
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Jolt.NET nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
// ----------------------------------------------------------------------------

using System.Xml;
using System.Xml.Linq;

namespace Doxie.Core.XmlComments;

/// <summary>
/// Reads one compiler documentation file. A document type definition is rejected. The file is
/// not checked against the historical documentation schema, so a file that uses later tags is
/// still loaded.
/// </summary>
public class XmlDocCommentReadPolicy : IXmlDocCommentReadPolicy
{
    private readonly XDocument xDocument;

    /// <summary>Opens the documentation file.</summary>
    /// <param name="xmlDocCommentsFullPath">Full path of the XML documentation file.</param>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="XmlException">The file is not XML, or it contains a document type definition.</exception>
    public XmlDocCommentReadPolicy(string xmlDocCommentsFullPath)
    {
        // Compiler documentation files contain tags the historical schema does not describe.
        // Rejecting the file here drops the whole assembly, so the document is loaded as XML only.
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit
        };

        using var stream = File.OpenRead(xmlDocCommentsFullPath);
        using var reader = XmlReader.Create(stream, settings);
        xDocument = XDocument.Load(reader);
    }

    /// <summary>
    /// The <c>member</c> element whose <c>name</c> attribute equals <paramref name="memberName"/>.
    /// </summary>
    /// <param name="memberName">The compiler member id, such as <c>M:Namespace.Type.Method</c>.</param>
    /// <returns>A copy of that element, or <see langword="null"/> when the name is absent.</returns>
    /// <exception cref="InvalidOperationException">More than one member has that name.</exception>
    public XElement ReadMember(string memberName)
    {
        var member = xDocument.Element(XmlDocCommentNames.DocElement)
            ?.Element(XmlDocCommentNames.MembersElement)
            ?.Elements(XmlDocCommentNames.MemberElement)
            .SingleOrDefault(e => e.Attribute(XmlDocCommentNames.NameAttribute)?.Value == memberName);

        // Copy the <member> element from the DOM.
        return member == null ? null : XElement.Load(member.CreateReader());
    }
}

internal class XmlDocCommentNames
{
    internal static readonly string DocElement = "doc";
    internal static readonly string AssemblyElement = "assembly";
    internal static readonly string NameElement = "name";
    internal static readonly string MembersElement = "members";
    internal static readonly string MemberElement = "member";
    internal static readonly string NameAttribute = NameElement;
}