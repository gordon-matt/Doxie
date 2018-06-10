// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Doxie.Core.Models;

namespace Doxie.Core.XmlComments
{
    public class DocParser
    {
        private XmlDocCommentReader xmlDocCommentReader;

        public AssemblyModel Parse(string assemblyFile, bool parseNamespace = true)
        {
            try
            {
                var namespaces = new List<NamespaceModel>();
                byte[] bytes = File.ReadAllBytes(assemblyFile);
                var assembly = Assembly.Load(bytes);

                if (parseNamespace)
                {
                    xmlDocCommentReader = new XmlDocCommentReader(assemblyFile.Replace(".dll", ".xml"));
                    FindTypes(assembly, namespaces);

                    namespaces = namespaces.OrderBy(o => o.Name).ToList();

                    foreach (var @namespace in namespaces)
                    {
                        @namespace.Classes = @namespace.Classes.OrderBy(c => c.Name).ToList();
                    }
                }

                var assemblyModel = new AssemblyModel
                {
                    Id = assembly.ManifestModule.ModuleVersionId,
                    Name = assembly.GetName().Name,
                    FullName = assembly.FullName,
                    Namespaces = namespaces
                };
                return assemblyModel;
            }
            catch (Exception x)
            {
                Trace.TraceError($"DLL {assemblyFile} : Parse Problem. {x.Message} => {x.Source}");
                return null;
            }
        }

        private void FindTypes(Assembly assembly, ICollection<NamespaceModel> namespaces)
        {
            var types = GetLoadableTypes(assembly)
                .Where(p => p.IsPublic || p.IsNestedPublic || p.IsVisible)
                .ToArray();

            foreach (var type in types)
            {
                try
                {
                    // The namespace is everything before this type name, e.g. [Docy.Core].XYZ
                    string typeNamespace = type.Namespace;

                    var @namespace = namespaces.FirstOrDefault(n => n.Name == typeNamespace);
                    if (@namespace == null)
                    {
                        @namespace = new NamespaceModel { Name = typeNamespace };
                        namespaces.Add(@namespace);
                    }

                    // Comments
                    var commentsElement = xmlDocCommentReader.GetComments(type);
                    var comments = GetCommonTags(commentsElement);

                    var typeBase = new TypeModel
                    {
                        Namespace = @namespace,
                        IsAbstract = type.IsAbstract,
                        IsPrimitive = type.IsPrimitive,
                        IsPublic = type.IsPublic,
                        IsSealed = type.IsSealed,
                        IsNested = type.IsNested,
                        ParentClass = type.BaseType != null ? type.BaseType.FullName : string.Empty,
                        Parents = GetParents(type),
                        Name = GetTypeName(type),
                        FullName = GetFullTypeName(type),
                        Example = comments.Example,
                        Remarks = comments.Remarks,
                        Returns = comments.Returns,
                        Summary = comments.Summary
                    };

                    typeBase.Constructors = GetConstructors(type, typeBase);
                    typeBase.Methods = GetMethods(type, typeBase);
                    typeBase.Properties = GetProperties(type, typeBase);

                    if (type.IsClass)
                    {
                        if (type.BaseType != null && type.BaseType == typeof(Delegate) || type.BaseType == typeof(MulticastDelegate))
                        {
                            typeBase.ObjectType = ObjectType.Delegate;
                            @namespace.Delegates.Add(typeBase);
                        }
                        else
                        {
                            typeBase.ObjectType = ObjectType.Class;
                            @namespace.Classes.Add(typeBase);
                        }
                    }
                    else if (type.IsEnum)
                    {
                        typeBase.ObjectType = ObjectType.Enumeration;
                        typeBase.Members = GetMembers(type);
                        @namespace.Enumerations.Add(typeBase);
                    }
                    else if (type.IsValueType)
                    {
                        typeBase.ObjectType = ObjectType.Structure;
                        @namespace.Structures.Add(typeBase);
                    }
                    else if (type.IsInterface)
                    {
                        typeBase.ObjectType = ObjectType.Interface;
                        @namespace.Interfaces.Add(typeBase);
                    }
                    else
                    {
                        typeBase.ObjectType = ObjectType.Delegate;
                        // TODO: Find out how to get delegate types
                        @namespace.Delegates.Add(typeBase);
                    }
                }
                catch (Exception x)
                {
                    Trace.TraceError($"Type {type} : Parse Problem. {x.Message} => {x.Source}");
                }
            }
        }

        private BaseCommentsModel GetCommonTags(XElement commentsElement)
        {
            var comments = new BaseCommentsModel();

            if (commentsElement != null)
            {
                // Example
                var current = commentsElement.Elements().FirstOrDefault(x => x.Name.LocalName == "example");
                if (current != null)
                {
                    comments.Example = current.Value.Trim();
                }

                // Remarks
                current = commentsElement.Elements().FirstOrDefault(x => x.Name.LocalName == "remarks");
                if (current != null)
                {
                    comments.Remarks = current.Value.Trim();
                }

                // Returns
                current = commentsElement.Elements().FirstOrDefault(x => x.Name.LocalName == "returns");
                if (current != null)
                {
                    comments.Returns = current.Value.Trim();
                }

                // Summary
                current = commentsElement.Elements().FirstOrDefault(x => x.Name.LocalName == "summary");
                if (current != null)
                {
                    comments.Summary = current.Value.Trim();
                }
            }

            return comments;
        }

        private ICollection<ConstructorModel> GetConstructors(Type type, TypeModel parent)
        {
            var list = new List<ConstructorModel>();

            foreach (var constructorInfo in type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                try
                {
                    var constructor = new ConstructorModel
                    {
                        Parent = parent,
                        Name = parent.Name,
                        ParentClass = parent.Id,
                        UseHashCodeForId = true,
                        Attributes = constructorInfo.Attributes.ToString(),
                    };

                    // Parameters
                    if (constructorInfo.IsGenericMethod)
                    {
                        var arguments = constructorInfo.GetGenericArguments().Select(genericType => genericType.Name).ToList();
                        constructor.Name = $"{constructorInfo.Name}<{string.Join(",", arguments)}>";
                    }

                    try
                    {
                        constructor.FullName = constructorInfo.ToString();

                        // Get common tags
                        var commentsElement = xmlDocCommentReader.GetComments(constructorInfo);
                        var comments = GetCommonTags(commentsElement);
                        constructor.Example = comments.Example;
                        constructor.Remarks = comments.Remarks;
                        constructor.Returns = comments.Returns;
                        constructor.Summary = comments.Summary;
                        constructor.Parameters = GetMethodParameters(constructorInfo.GetParameters(), commentsElement);
                    }
                    catch (Exception x)
                    {
                        constructor.LoadError = true;
                        constructor.FullName = x.Message;
                    }

                    list.Add(constructor);
                }
                catch (Exception x)
                {
                    Trace.TraceError($"constructor {constructorInfo.Name} : Parse Problem. {x.Message} => {x.Source}");
                }
            }

            list = list.OrderBy(m => m.Name).ToList();
            return list;
        }

        private string GetFriendlyGenericName(Type paramType)
        {
            string result;

            if (paramType.IsGenericType || paramType.IsGenericParameter)
            {
                // Turn #1 into #3
                // #1 System.Linq.Expressions.Expression`1[[System.Func`2[[Roadkill.Core.User, Roadkill.Core, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null],[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                // #2 System.Linq.Expressions.Expression`1[[System.Func`2[[Roadkill.Core.User],[System.Object]]]]
                // #3 System.Linq.Expressions.Expression<Func<User,object>>

                // todo : trouver un contournement
                //var csharpProvider = CodeDomProvider.CreateProvider("C#");

                //var typeReference = new CodeTypeReference(paramType);
                //var variableDeclaration = new CodeVariableDeclarationStatement(typeReference, "dummy");
                var stringBuilder = new StringBuilder();
                //using (var writer = new StringWriter(stringBuilder))
                //{
                //    csharpProvider.GenerateCodeFromStatement(variableDeclaration, writer, new CodeGeneratorOptions());
                //}

                stringBuilder.Replace(" dummy;", null);
                result = stringBuilder.ToString();
            }
            else
            {
                result = paramType.Name;
            }

            return result.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        private string GetFullTypeName(Type type)
        {
            var result = type.FullName;

            if (string.IsNullOrEmpty(result))
            {
                result = type.Name;
            }

            if (type.IsGenericParameter || (type.IsGenericType))
            {
                result = GetFriendlyGenericName(type);
            }

            return result;
        }

        private ICollection<MemberSummaryModel> GetMembers(Type type)
        {
            // For enumerations
            var list = (from info in type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                        where info.Name != "value__"
                        let element = xmlDocCommentReader.GetComments(info)
                        let comments = GetCommonTags(element)
                        select new MemberSummaryModel
                        {
                            Name = info.Name,
                            Description = comments.Summary
                        }).OrderBy(m => m.Name).ToList();

            return list;
        }

        private ICollection<ParameterModel> GetMethodParameters(IEnumerable<ParameterInfo> parameters, XElement element)
        {
            IEnumerable<XElement> paramElements = new List<XElement>();

            if (element != null)
            {
                paramElements = element.Elements().Where(e => e.Name.LocalName == "param");
            }

            var list = new List<ParameterModel>();
            foreach (var parameterInfo in parameters)
            {
                var parameter = new ParameterModel
                {
                    Name = parameterInfo.Name,
                    IsOut = parameterInfo.IsOut,
                    IsRetval = parameterInfo.IsRetval,
                    Type = GetTypeName(parameterInfo.ParameterType),
                    TypeFullName = GetFullTypeName(parameterInfo.ParameterType)
                };

                var current = paramElements.FirstOrDefault(e => e.Attribute("name") != null && e.Attribute("name")?.Value == parameter.Name);
                if (current != null)
                {
                    // TODO: get attributes
                    parameter.Description = current.Value;

                    if (!string.IsNullOrEmpty(parameter.Description))
                    {
                        parameter.Description = parameter.Description.Trim();
                    }
                }

                list.Add(parameter);
            }

            return list;
        }

        private ICollection<MethodModel> GetMethods(Type type, TypeModel parent)
        {
            var list = new List<MethodModel>();

            // Used for ID generation
            var methodNames = new List<string>();

            // Get only methods for this object, none of the inherited methods.
            foreach (var methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                try
                {
                    if (!methodInfo.IsSpecialName)
                    {
                        var method = new MethodModel
                        {
                            Parent = parent,
                            ParentClass = parent.Id,
                            Name = methodInfo.Name,
                        };

                        // Contains overloads - ID is the hashcode
                        if (methodNames.Contains(methodInfo.Name))
                        {
                            method.UseHashCodeForId = true;
                        }

                        methodNames.Add(methodInfo.Name);

                        if (methodInfo.IsGenericMethod)
                        {
                            var arguments = methodInfo.GetGenericArguments().Select(genericType => genericType.Name).ToList();

                            method.Name = $"{methodInfo.Name}<{string.Join(",", arguments)}>";
                            method.UseHashCodeForId = true;
                        }

                        try
                        {
                            method.FullName = methodInfo.ToString();
                            method.ReturnType = GetTypeName(methodInfo.ReturnType);
                            method.ReturnTypeFullName = GetFullTypeName(methodInfo.ReturnType);

                            // Get common tags
                            var element =
                                xmlDocCommentReader.GetComments(methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo);
                            var comments = GetCommonTags(element);
                            method.Example = comments.Example;
                            method.Remarks = comments.Remarks;
                            method.Returns = comments.Returns;
                            method.Summary = comments.Summary;
                            method.Parameters = GetMethodParameters(methodInfo.GetParameters(), element); // Parameters
                        }
                        catch (Exception x)
                        {
                            method.LoadError = true;
                            method.FullName = x.Message;
                        }

                        list.Add(method);
                    }
                }
                catch (Exception x)
                {
                    Trace.TraceError($"Type {type} : Parse methods Problem. {x.Message} => {x.Source}");
                }
            }

            return list.OrderBy(x => x.Name).ToList();
        }

        private ICollection<TypeSummaryModel> GetParents(Type type)
        {
            var parents = new List<TypeSummaryModel>();

            var current = type;
            while (current.BaseType != null)
            {
                var typeSummary = new TypeSummaryModel
                {
                    Name = current.BaseType.Name,
                    FullName = GetFullTypeName(current.BaseType)
                };

                parents.Add(typeSummary);
                current = current.BaseType;
            }

            parents.Reverse();
            return parents;
        }

        private ICollection<PropertyModel> GetProperties(Type type, TypeModel parent)
        {
            var list = new List<PropertyModel>();

            // Get both public and protected properties, and no inherited ones unless overridden
            foreach (var propertyInfo in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                try
                {
                    var property = new PropertyModel
                    {
                        Name = propertyInfo.Name,
                        Parent = parent,
                        ParentClass = parent.Id,
                        Attributes = propertyInfo.Attributes.ToString(),
                    };

                    try
                    {
                        property.FullName = propertyInfo.ToString();
                        property.Type = GetTypeName(propertyInfo.PropertyType);
                        property.TypeFullName = GetFullTypeName(propertyInfo.PropertyType);

                        var commentsElement = xmlDocCommentReader.GetComments(propertyInfo);
                        var comments = GetCommonTags(commentsElement);
                        property.Example = comments.Example;
                        property.Remarks = comments.Remarks;
                        property.Returns = comments.Returns;
                        property.Summary = comments.Summary;
                    }
                    catch (Exception x)
                    {
                        property.LoadError = true;
                        property.FullName = x.Message;
                    }
                    list.Add(property);
                }
                catch (Exception x)
                {
                    Trace.TraceError($"Type {propertyInfo.Name} : Parse Property Problem. {x.Message} => {x.Source}");
                }
            }

            return list.OrderBy(m => m.Name).ToList();
        }

        /// <summary>
        /// Get the type name with a sprinkle of magic dust if it's a generic type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetTypeName(Type type)
        {
            string result = type.Name;

            if (type.IsGenericParameter || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                result = GetFriendlyGenericName(type);
            }

            return result;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException x)
            {
                return x.Types.Where(t => t != null);
            }
        }
    }
}