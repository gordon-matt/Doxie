using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Doxie.Core.Helpers.XmlHelper.Interfaces;

namespace Doxie.Core.Helpers.XmlHelper
{
    public class XmlDocCommentReadPolicy : IXmlDocCommentReadPolicy
    {
        private readonly XmlReaderSettings xmlReaderSettings;
        private readonly XDocument xDocument;

        private string XmlDocCommentsFullPath { get; }

        public XmlDocCommentReadPolicy(string xmlDocCommentsFullPath)
        {
            xmlReaderSettings = new XmlReaderSettings { ValidationType = ValidationType.Schema };

            using (var schema = typeof(XmlDocCommentReadPolicy).Assembly.GetManifestResourceStream("Doxie.Core.DocComments.xsd"))
            {
                xmlReaderSettings.Schemas.Add(XmlSchema.Read(schema ?? throw new InvalidOperationException("DocComments.xsd not found"), null));
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