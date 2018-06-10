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
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Doxie.Core.XmlComments
{
    /// <summary>
    /// Provides methods to convert between representations of a type.
    /// </summary>
    public static class Convert
    {
        #region Private Members

        private const string SZArrayTypeSuffix = "[]";
        private const string ArrayElementTypeDimension = "0:";
        private static readonly string ArrayElementTypeDimensionDelimited = ArrayElementTypeDimension + ',';

        private static readonly string[] ConversionOperatorNames = { "op_Explicit", "op_Implicit" }; // Keep this lexicographically sorted.

        private static readonly IDictionary<Type, string> XDCMemberPrefixes;
        private static readonly ParameterInfo[] EmptyParameterList;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Initializes the static state of the <see cref="Convert"/> class.
        /// </summary>
        static Convert()
        {
            EmptyParameterList = new ParameterInfo[0];

            const string methodPrefix = "M:";
            XDCMemberPrefixes = new Dictionary<Type, string>
            {
                {typeof(Type), "T:"},
                {typeof(EventInfo), "E:"},
                {typeof(PropertyInfo), "P:"},
                {typeof(MemberInfo), "F:"},
                {typeof(FieldInfo), "F:"},
                {typeof(MethodInfo), methodPrefix},
                {typeof(ConstructorInfo), methodPrefix}
            };
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given <see cref="System.Type"/>.
        /// </summary>
        ///
        /// <param name="type">
        /// The <see cref="System.Type"/> to convert.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        public static string ToXmlDocCommentMember(Type type)
        {
            var builder = new StringBuilder();
            return AppendXDCFullTypeNameTo(builder, type)
                .Insert(0, XDCMemberPrefixes[typeof(Type)])
                .ToString();
        }

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given <see cref="System.Reflection.EventInfo"/>.
        /// </summary>
        ///
        /// <param name="eventInfo">
        /// The <see cref="System.Reflection.EventInfo"/> to convert.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        public static string ToXmlDocCommentMember(EventInfo eventInfo)
        {
            return ToXmlDocCommentMember(eventInfo, EmptyParameterList).ToString();
        }

        public static string ToXmlDocCommentMember(MemberInfo memberInfo)
        {
            return ToXmlDocCommentMember(memberInfo, EmptyParameterList).ToString();
        }

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given <see cref="System.Reflection.FieldInfo"/>.
        /// </summary>
        ///
        /// <param name="fieldInfo">
        /// The <see cref="System.Reflection.FieldInfo"/> to convert.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        public static string ToXmlDocCommentMember(FieldInfo fieldInfo)
        {
            return ToXmlDocCommentMember(fieldInfo, EmptyParameterList).ToString();
        }

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given <see cref="System.Reflection.PropertyInfo"/>.
        /// </summary>
        ///
        /// <param name="propertyInfo">
        /// The <see cref="System.Reflection.PropertyInfo"/> to convert.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        public static string ToXmlDocCommentMember(PropertyInfo propertyInfo)
        {
            return ToXmlDocCommentMember(propertyInfo, propertyInfo.GetIndexParameters()).ToString();
        }

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given <see cref="System.Reflection.ConstructorInfo"/>.
        /// </summary>
        ///
        /// <param name="constructorInfo">
        /// The <see cref="System.Reflection.ConstructorInfo"/> to convert.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        public static string ToXmlDocCommentMember(ConstructorInfo constructorInfo)
        {
            int namePosition;
            var builder = ToXmlDocCommentMember(constructorInfo, constructorInfo.GetParameters(), out namePosition);
            builder[namePosition] = '#'; // Replaces . with # in ctor name.

            return builder.ToString();
        }

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given <see cref="System.Reflection.MethodInfo"/>.
        /// </summary>
        ///
        /// <param name="methodInfo">
        /// The <see cref="System.Reflection.MethodInfo"/> to convert.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        public static string ToXmlDocCommentMember(MethodInfo methodInfo)
        {
            var builder = ToXmlDocCommentMember(methodInfo, methodInfo.GetParameters());

            if (Array.BinarySearch(ConversionOperatorNames, methodInfo.Name) >= 0)
            {
                builder.Append('~');
                AppendXDCParameterTypesTo(builder, new[] { methodInfo.ReturnType });
            }

            return builder.ToString();
        }

        /// <summary>
        /// Retrieves the type of each <see cref="System.Reflection.ParameterInfo"/> object
        /// from the given array.
        /// </summary>
        ///
        /// <param name="parameters">
        /// The parameters to convert.
        /// </param>
        ///
        /// <returns>
        /// An array of <see cref="System.Type"/> objects representing the
        /// corresponding parameter types.
        /// </returns>
        public static Type[] ToParameterTypes(ParameterInfo[] parameters)
        {
            return ToParameterTypes(parameters, Type.EmptyTypes, Type.EmptyTypes);
        }

        /// <summary>
        /// Retrieves the name of each <see cref="System.Type"/> object from the given array.
        /// </summary>
        ///
        /// <param name="types">
        /// The types to convert.
        /// </param>
        ///
        /// <returns>
        /// An array of strings representing the corresponding type names.
        /// </returns>
        public static string[] ToTypeNames(Type[] types)
        {
            return Array.ConvertAll(types, type => type.Name);
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Retrieves the type of each <see cref="System.Reflection.ParameterInfo"/> object
        /// from the given array.
        /// </summary>
        ///
        /// <param name="parameters">
        /// The parameters to convert.
        /// </param>
        ///
        /// <param name="genericTypeArguments">
        /// The generic arguments from the declaring type of the parameter's method.
        /// </param>
        ///
        /// <returns>
        /// An array of <see cref="System.Type"/> objects representing the
        /// corresponding parameter types.
        /// </returns>
        ///
        /// <remarks>
        /// Substitutes the corresponding type from the given generic type argument array when a
        /// parameter type is deemed to be a generic type argument.
        /// </remarks>
        internal static Type[] ToParameterTypes(ParameterInfo[] parameters, Type[] genericTypeArguments)
        {
            return ToParameterTypes(parameters, genericTypeArguments, Type.EmptyTypes);
        }

        /// <summary>
        /// Retrieves the type of each <see cref="System.Reflection.ParameterInfo"/> object
        /// from the given array.
        /// </summary>
        ///
        /// <param name="parameters">
        /// The parameters to convert.
        /// </param>
        ///
        /// <param name="genericTypeArguments">
        /// The generic arguments from the declaring type of the parameter's method.
        /// </param>
        ///
        /// <param name="genericMethodArguments">
        /// The generic arguments from the parameter's method.
        /// </param>
        ///
        /// <returns>
        /// An array of <see cref="System.Type"/> objects representing the
        /// corresponding parameter types.
        /// </returns>
        ///
        /// <remarks>
        /// Substitutes the corresponding type from the given generic argument arrays when a
        /// parameter type is deemed to be a generic type/method argument.
        /// </remarks>
        internal static Type[] ToParameterTypes(ParameterInfo[] parameters, Type[] genericTypeArguments,
            Type[] genericMethodArguments)
        {
            return Array.ConvertAll(parameters,
                methodParam =>
                    ToMethodSignatureType(methodParam.ParameterType, genericTypeArguments, genericMethodArguments));
        }

        /// <summary>
        /// Converts a given type to one that represents a defined type
        /// that participates in an external method signature.  Refers to
        /// the type from a generic type parameter collection when the
        /// given type is deemed to begeneric.
        /// </summary>
        ///
        /// <param name="parameterType">
        /// The type to convert.
        /// </param>
        ///
        /// <param name="genericTypeArguments">
        /// The generic arguments from the declaring type of the parameter's method.
        /// </param>
        ///
        /// <param name="genericMethodArguments">
        /// The generic arguments from the parameter's method.
        /// </param>
        internal static Type ToMethodSignatureType(Type parameterType, Type[] genericTypeArguments,
            Type[] genericMethodArguments)
        {
            if (parameterType.IsGenericParameter)
            {
                if (parameterType.DeclaringMethod != null && genericMethodArguments.Length > 0)
                {
                    return genericMethodArguments[parameterType.GenericParameterPosition];
                }

                if (genericTypeArguments.Length > 0)
                {
                    return genericTypeArguments[parameterType.GenericParameterPosition];
                }
            }

            return parameterType;
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given type.
        /// </summary>
        ///
        /// <typeparam name="TMember">
        /// The type of the member to convert.
        /// </typeparam>
        ///
        /// <param name="member">
        /// The member from which the string is created.
        /// </param>
        ///
        /// <param name="memberParameters">
        /// The parameters to the member, if any.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        private static StringBuilder ToXmlDocCommentMember<TMember>(TMember member, ParameterInfo[] memberParameters)
            where TMember : MemberInfo
        {
            int dummy;
            return ToXmlDocCommentMember(member, memberParameters, out dummy);
        }

        /// <summary>
        /// Creates the XML doc comment member reference string
        /// for a given type, indexing the starting position of the
        /// converted member name.
        /// </summary>
        ///
        /// <typeparam name="TMember">
        /// The type of the member to convert.
        /// </typeparam>
        ///
        /// <param name="member">
        /// The member from which the string is created.
        /// </param>
        ///
        /// <param name="memberParameters">
        /// The parameters to the member, if any.
        /// </param>
        ///
        /// <param name="namePosition">
        /// The position in the resulting string that indexes that starting
        /// position of the member name.
        /// </param>
        ///
        /// <returns>
        /// A string containing the requested member reference.
        /// </returns>
        private static StringBuilder ToXmlDocCommentMember<TMember>(TMember member, ParameterInfo[] memberParameters,
            out int namePosition)
            where TMember : MemberInfo
        {
            var builder = new StringBuilder();
            AppendXDCFullTypeNameTo(builder, member.DeclaringType)
                .Insert(0, XDCMemberPrefixes[typeof(TMember)])
                .Append('.');

            namePosition = builder.Length;
            builder.Append(member.Name);

            var methodInfo = member as MethodInfo;
            if (methodInfo != null && methodInfo.IsGenericMethod)
            {
                builder.Append("``");
                builder.Append(methodInfo.GetGenericArguments().Length);
            }

            if (memberParameters.Length > 0)
            {
                builder.Append('(');
                AppendXDCParameterTypesTo(builder, ToParameterTypes(memberParameters)).Append(')');
            }

            return builder;
        }

        /// <summary>
        /// Appends the XML doc comment representation of the full name of the given
        /// <see cref="System.Type"/> to a given <see cref="System.Text.StringBuilder"/>.
        /// </summary>
        ///
        /// <param name="builder">
        /// The <see cref="System.Text.StringBuilder"/> to which the type name is appended.
        /// </param>
        ///
        /// <param name="type">
        /// The type whose name is appended.
        /// </param>
        ///
        /// <returns>
        /// The <see cref="System.Text.StringBuilder"/> parameter, modified by the appended name.
        /// </returns>
        ///
        /// <remarks>
        /// Includes the appropriate symbol when the given type is generic.
        /// </remarks>
        private static StringBuilder AppendXDCFullTypeNameTo(StringBuilder builder, Type type)
        {
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            return AppendNormalizedXDCTypeNameTo(builder, type);
        }

        /// <summary>
        /// Appends the XML doc comment representation of the abbreviated name of the given
        /// <see cref="System.Type"/> to a given <see cref="System.Text.StringBuilder"/>.
        /// </summary>
        ///
        /// <param name="builder">
        /// The <see cref="System.Text.StringBuilder"/> to which the type name is appended.
        /// </param>
        ///
        /// <param name="type">
        /// The type whose name is appended.
        /// </param>
        ///
        /// <returns>
        /// The <see cref="System.Text.StringBuilder"/> parameter, modified by the appended name.
        /// </returns>
        ///
        /// <remarks>
        /// Does not include the appropriate symbol when the given type is generic.
        /// </remarks>
        private static StringBuilder AppendXDCTypeNameTo(StringBuilder builder, Type type)
        {
            var typeNameBuilder = new StringBuilder();
            AppendNormalizedXDCTypeNameTo(typeNameBuilder, type);

            if (type.IsGenericType)
            {
                if (type.FullName != null) typeNameBuilder.Length = type.FullName.IndexOf('`');
            }

            return builder.Append(typeNameBuilder);
        }

        /// <summary>
        /// Appends the XML doc comment representation of the normalized name of the given
        /// <see cref="System.Type"/> to a given <see cref="System.Text.StringBuilder"/>.
        /// </summary>
        ///
        /// <param name="builder">
        /// The <see cref="System.Text.StringBuilder"/> to which the type name is appended.
        /// </param>
        ///
        /// <param name="type">
        /// The type whose name is normalized and appended.
        /// </param>
        ///
        /// <returns>
        /// The <see cref="System.Text.StringBuilder"/> parameter, modified by the appended name.
        /// </returns>
        ///
        /// <remarks>
        /// Handles appending of both top-level and nested type types.
        /// </remarks>
        private static StringBuilder AppendNormalizedXDCTypeNameTo(StringBuilder builder, Type type)
        {
            return type.IsNested ? builder.Append(type.FullName.Replace('+', '.')) : builder.Append(type.FullName);
        }

        /// <summary>
        /// Appends the XML doc comment representation of the name of each given
        /// <see cref="System.Type"/> to a given <see cref="System.Text.StringBuilder"/>.
        /// </summary>
        ///
        /// <param name="builder">
        /// The <see cref="System.Text.StringBuilder"/> to which the type names are appended.
        /// </param>
        ///
        /// <param name="parameterTypes">
        /// The types whose names are appended.
        /// </param>
        ///
        /// <returns>
        /// The <see cref="System.Text.StringBuilder"/> parameter, modified by the appended names.
        /// </returns>
        private static StringBuilder AppendXDCParameterTypesTo(StringBuilder builder, Type[] parameterTypes)
        {
            for (var i = 0; i < parameterTypes.Length; ++i)
            {
                var parameterModifier = ReduceToElementType(ref parameterTypes[i]);
                if (parameterTypes[i].IsGenericType)
                {
                    AppendXDCTypeNameTo(builder, parameterTypes[i].GetGenericTypeDefinition()).Append('{');
                    AppendXDCParameterTypesTo(builder, parameterTypes[i].GetGenericArguments()).Append('}');
                }
                else if (parameterTypes[i].IsGenericParameter)
                {
                    builder.Append('`');
                    if (parameterTypes[i].DeclaringMethod != null)
                    {
                        builder.Append('`');
                    }

                    builder.Append(parameterTypes[i].GenericParameterPosition);
                }
                else
                {
                    AppendXDCTypeNameTo(builder, parameterTypes[i]);
                }

                builder.Append(parameterModifier).Append(',');
            }

            builder.Length -= 1;
            return builder;
        }

        /// <summary>
        /// Modifies the given <see cref="System.Type"/> by reducing it to
        /// its inner-most element type (removes any array or pointer decorations),
        /// and creates the type's XML doc comment reprentation.
        /// </summary>
        ///
        /// <param name="type">
        /// The <see cref="System.Type"/> to reduce.
        /// </param>
        ///
        /// <returns>
        /// The XML doc comment representation of the given type's name.
        /// </returns>
        ///
        /// <remarks>
        /// The element type is the type of the given type, excluding
        /// any array, pointer or by-ref modifiers.
        /// </remarks>
        private static string ReduceToElementType(ref Type type)
        {
            var builder = new StringBuilder();
            while (type != null && type.IsByRef)
            {
                // ELEMENT_TYPE_BYREF
                builder.Append('@');
                type = type.GetElementType();
            }

            while (type != null && type.IsArray)
            {
                var rank = type.GetArrayRank();
                if (rank == 1)
                {
                    // ELEMENT_TYPE_SZARRAY
                    builder.Insert(0, SZArrayTypeSuffix);
                }
                else
                {
                    // ELEMENT_TYPE_ARRAY
                    builder.Insert(0, ']')
                        .Insert(0, ArrayElementTypeDimension)
                        .Insert(0, ArrayElementTypeDimensionDelimited, rank - 1)
                        .Insert(0, '[');
                }

                type = type.GetElementType();
            }

            while (type != null && type.IsPointer)
            {
                // ELEMENT_TYPE_PTR
                builder.Insert(0, '*');
                type = type.GetElementType();
            }

            return builder.ToString();
        }

        #endregion Private Methods
    }
}