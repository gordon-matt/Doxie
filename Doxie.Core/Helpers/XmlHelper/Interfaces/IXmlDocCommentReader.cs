using System;
using System.Reflection;
using System.Xml.Linq;

namespace Doxie.Core.Helpers.XmlHelper.Interfaces
{
    /// <summary>
    /// An internal interface supporting the testing of objects
    /// that have and/or use an <see cref="XmlDocCommentReader"/>.
    /// </summary>
    internal interface IXmlDocCommentReader
    {
        XElement GetComments(ConstructorInfo constructorInfo);

        XElement GetComments(EventInfo eventInfoInfo);

        XElement GetComments(FieldInfo fieldInfo);

        XElement GetComments(MethodInfo methodInfo);

        XElement GetComments(MemberInfo memberInfo);

        XElement GetComments(PropertyInfo propertyInfo);

        XElement GetComments(Type type);
    }
}