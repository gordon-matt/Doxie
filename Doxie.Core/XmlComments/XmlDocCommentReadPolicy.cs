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

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Doxie.Core.XmlComments
{
    public class XmlDocCommentReadPolicy : IXmlDocCommentReadPolicy
    {
        private readonly XmlReaderSettings xmlReaderSettings;
        private readonly XDocument xDocument;

        private string XmlDocCommentsFullPath { get; }

        public XmlDocCommentReadPolicy(string xmlDocCommentsFullPath)
        {
            xmlReaderSettings = new XmlReaderSettings { ValidationType = ValidationType.Schema };

            using (var schema = typeof(XmlDocCommentReadPolicy).Assembly.GetManifestResourceStream("Doxie.Core.XmlComments.Schema.xsd"))
            {
                xmlReaderSettings.Schemas.Add(XmlSchema.Read(schema ?? throw new InvalidOperationException("Schema.xsd not found"), null));
            }

            XmlDocCommentsFullPath = xmlDocCommentsFullPath;
            using (var reader = CreateXmlReader())
            {
                xDocument = XDocument.Load(reader);
            }
        }

        protected XmlReader CreateXmlReader()
        {
            return XmlReader.Create(File.OpenText(XmlDocCommentsFullPath), xmlReaderSettings);
        }

        /// <summary>
        /// <see cref="IXmlDocCommentReadPolicy.ReadMember"/>
        /// </summary>
        XElement IXmlDocCommentReadPolicy.ReadMember(string memberName)
        {
            XElement member = xDocument.Element(XmlDocCommentNames.DocElement)
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
}