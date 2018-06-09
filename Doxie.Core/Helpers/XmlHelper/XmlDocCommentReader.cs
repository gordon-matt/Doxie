using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Doxie.Core.Helpers.XmlHelper.Interfaces;

namespace Doxie.Core.Helpers.XmlHelper
{
    // Represents a factory method for creating types that implement
    // the IXmlDocCommentReadPolicy interface.  The string parameter
    // contains the full path of the XML doc comment file that is to
    // be read by the policy.
    using CreateReadPolicyDelegate = Func<string, IXmlDocCommentReadPolicy>;

    /// <summary>
    /// Provides methods to retrieve the XML Documentation Comments for an
    /// object having a metadata type from the System.Reflection namespace.
    /// </summary>
    public sealed class XmlDocCommentReader : IXmlDocCommentReader
    {
        #region Private Members

        private static readonly CreateReadPolicyDelegate CreateDefaultReadPolicy = path => new XmlDocCommentReadPolicy(path);

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="XmlDocCommentReader"/> class
        /// with a given path to the XML doc comments.
        /// </summary>
        ///
        /// <param name="docCommentsFullPath">
        /// The full path of the XML doc comments.
        /// </param>
        public XmlDocCommentReader(string docCommentsFullPath)
            : this(docCommentsFullPath, CreateDefaultReadPolicy) { }

        /// <summary>
        /// Creates a new instance of the <see cref="XmlDocCommentReader"/> class
        /// with a given path to the XML doc comments, and configures the reader
        /// to use a user-defined read policy.
        /// </summary>
        ///
        /// <param name="docCommentsFullPath">
        /// The full path of the XML doc comments.
        /// </param>
        ///
        /// <param name="createReadPolicy">
        /// A factory method that accepts the full path to an XML doc comments file,
        /// returning a user-defined read policy.
        /// </param>
        public XmlDocCommentReader(string docCommentsFullPath, CreateReadPolicyDelegate createReadPolicy)
            : this(docCommentsFullPath, createReadPolicy(docCommentsFullPath)) { }

        /// <summary>
        /// Creates a new instance of the <see cref="XmlDocCommentReader"/> class
        /// with a given path to the XML doc comments, and configures the reader
        /// to use a user-defined read policy.
        /// </summary>
        ///
        /// <param name="docCommentsFullPath">
        /// The full path of the XML doc comments.
        /// </param>
        ///
        /// <param name="readPolicy">
        /// The doc comment read policy.
        /// </param>
        ///
        /// <remarks>
        /// Used internally by test code to override file IO operations.
        /// </remarks>
        ///
        /// <exception cref="System.IO.FileNotFoundException">
        /// <paramref name="docCommentsFullPath"/> could does not exist or is inaccessible.
        /// </exception>
        internal XmlDocCommentReader(string docCommentsFullPath, IXmlDocCommentReadPolicy readPolicy)
        {
            if (!File.Exists(docCommentsFullPath))
            {
                throw new FileNotFoundException($"File, '{docCommentsFullPath}' not found!", docCommentsFullPath);
            }

            FullPath = docCommentsFullPath;
            ReadPolicy = readPolicy;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Retrieves the xml doc comments for a given <see cref="System.Type"/>.
        /// </summary>
        ///
        /// <param name="type">
        /// The <see cref="System.Type"/> for which the doc comments are retrieved.
        /// </param>
        ///
        /// <returns>
        /// An <see cref="XElement"/> containing the requested XML doc comments,
        /// or NULL if none were found.
        /// </returns>
        public XElement GetComments(Type type)
        {
            return ReadPolicy.ReadMember(Convert.ToXmlDocCommentMember(type));
        }

        /// <summary>
        /// Retrieves the xml doc comments for a given <see cref="System.Reflection.EventInfo"/>.
        /// </summary>
        ///
        /// <param name="eventInfo">
        /// The <see cref="System.Reflection.EventInfo"/> for which the doc comments are retrieved.
        /// </param>
        ///
        /// <returns>
        /// An <see cref="XElement"/> containing the requested XML doc comments,
        /// or NULL if none were found.
        /// </returns>
        public XElement GetComments(EventInfo eventInfo)
        {
            return ReadPolicy.ReadMember(Convert.ToXmlDocCommentMember(eventInfo));
        }

        /// <summary>
        /// Retrieves the xml doc comments for a given <see cref="System.Reflection.FieldInfo"/>.
        /// </summary>
        ///
        /// <param name="fieldInfo">
        /// The <see cref="System.Reflection.FieldInfo"/> for which the doc comments are retrieved.
        /// </param>
        ///
        /// <returns>
        /// An <see cref="XElement"/> containing the requested XML doc comments,
        /// or NULL if none were found.
        /// </returns>
        public XElement GetComments(FieldInfo fieldInfo)
        {
            return ReadPolicy.ReadMember(Convert.ToXmlDocCommentMember(fieldInfo));
        }

        /// <summary>
        /// Retrieves the xml doc comments for a given <see cref="System.Reflection.PropertyInfo"/>.
        /// </summary>
        ///
        /// <param name="propertyInfo">
        /// The <see cref="System.Reflection.PropertyInfo"/> for which the doc comments are retrieved.
        /// </param>
        ///
        /// <returns>
        /// An <see cref="XElement"/> containing the requested XML doc comments,
        /// or NULL if none were found.
        /// </returns>
        public XElement GetComments(PropertyInfo propertyInfo)
        {
            return ReadPolicy.ReadMember(Convert.ToXmlDocCommentMember(propertyInfo));
        }

        /// <summary>
        /// Retrieves the xml doc comments for a given <see cref="System.Reflection.ConstructorInfo"/>.
        /// </summary>
        ///
        /// <param name="constructorInfo">
        /// The <see cref="System.Reflection.ConstructorInfo"/> for which the doc comments are retrieved.
        /// </param>
        ///
        /// <returns>
        /// An <see cref="XElement"/> containing the requested XML doc comments,
        /// or NULL if none were found.
        /// </returns>
        public XElement GetComments(ConstructorInfo constructorInfo)
        {
            return ReadPolicy.ReadMember(Convert.ToXmlDocCommentMember(constructorInfo));
        }

        /// <summary>
        /// Retrieves the xml doc comments for a given <see cref="System.Reflection.MethodInfo"/>.
        /// </summary>
        ///
        /// <param name="methodInfo">
        /// The <see cref="System.Reflection.MethodInfo"/> for which the doc comments are retrieved.
        /// </param>
        ///
        /// <returns>
        /// An <see cref="XElement"/> containing the requested XML doc comments,
        /// or NULL if none were found.
        /// </returns>
        public XElement GetComments(MethodInfo methodInfo)
        {
            return ReadPolicy.ReadMember(Convert.ToXmlDocCommentMember(methodInfo));
        }

        public XElement GetComments(MemberInfo memberInfo)
        {
            return ReadPolicy.ReadMember(Convert.ToXmlDocCommentMember(memberInfo));
        }

        #endregion Public Methods

        #region Public Properties

        /// <summary>
        /// Gets the full path to the XML doc comments file that is
        /// read by the reader.
        /// </summary>
        public string FullPath { get; }

        #endregion Public Properties

        #region Non-Public Properties

        /// <summary>
        /// Gets the read policy used by the class.
        /// </summary>
        internal IXmlDocCommentReadPolicy ReadPolicy { get; }

        #endregion Non-Public Properties
    }
}