// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Doxie.Core.Models;

namespace Doxie.Core.XmlComments;

public class DocParser
{
    private static readonly string[] FrameworkPaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared", "Microsoft.NETCore.App"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared", "Microsoft.AspNetCore.App")
    ];

    private readonly Dictionary<string, XmlDocCommentReader> commentReaders = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> missingDocumentation = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> documentationDirectories = [];
    private readonly HashSet<string> assembliesBeingResolved = new(StringComparer.OrdinalIgnoreCase);

    private XmlDocCommentReader xmlDocCommentReader;
    private string sourceDirectory;

    /// <summary>
    /// Loads every selected documentation file before parsing, so an implementing
    /// class can inherit comments from an interface that lives in another assembly.
    /// </summary>
    public void PrepareDocumentation(IEnumerable<string> assemblyFiles)
    {
        commentReaders.Clear();
        missingDocumentation.Clear();
        documentationDirectories.Clear();
        foreach (string assemblyFile in assemblyFiles)
        {
            RememberDirectory(Path.GetDirectoryName(assemblyFile));
            RegisterDocumentation(assemblyFile);
        }
    }

    public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        // First try normal loading with assembly resolution
        AppDomain.CurrentDomain.AssemblyResolve += ResolveFrameworkAssembly;
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null);
        }
        catch (Exception)
        {
            // If normal loading fails completely, try metadata-only approach
            return GetTypesMetadataOnly(assembly.Location);
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveFrameworkAssembly;
        }
    }

    public AssemblyModel Parse(string assemblyFile, bool parseXmlCommentsFile = true)
    {
        xmlDocCommentReader = null;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveDocumentationAssembly;

        try
        {
            byte[] bytes = File.ReadAllBytes(assemblyFile);
            var assembly = Assembly.Load(bytes);

            sourceDirectory = Path.GetDirectoryName(assemblyFile);
            RememberDirectory(sourceDirectory);
            if (parseXmlCommentsFile)
            {
                string assemblyName = assembly.GetName().Name;
                if (assemblyName == null || !commentReaders.TryGetValue(assemblyName, out xmlDocCommentReader))
                {
                    xmlDocCommentReader = OpenDocumentation(assemblyFile);
                    if (xmlDocCommentReader != null && assemblyName != null)
                    {
                        commentReaders[assemblyName] = xmlDocCommentReader;
                    }
                }
            }

            var namespaces = new List<NamespaceModel>();
            FindTypes(assembly, namespaces);
            namespaces = namespaces.OrderBy(o => o.Name).ToList();

            foreach (var @namespace in namespaces)
            {
                @namespace.Classes = @namespace.Classes.OrderBy(c => c.Name).ToList();
                @namespace.Structures = @namespace.Structures.OrderBy(c => c.Name).ToList();
                @namespace.Interfaces = @namespace.Interfaces.OrderBy(c => c.Name).ToList();
                @namespace.Enumerations = @namespace.Enumerations.OrderBy(c => c.Name).ToList();
                @namespace.Delegates = @namespace.Delegates.OrderBy(c => c.Name).ToList();
            }

            return new AssemblyModel
            {
                Id = assembly.ManifestModule.ModuleVersionId,
                Name = assembly.GetName().Name,
                FullName = assembly.FullName,
                Namespaces = namespaces
            };
        }
        catch (Exception x)
        {
            Trace.TraceError($"DLL {assemblyFile} : Parse Problem. {x.Message} => {x.Source}");
            return new AssemblyModel
            {
                Id = StableGuid(assemblyFile),
                Name = Path.GetFileNameWithoutExtension(assemblyFile),
                FullName = $"{assemblyFile}: {x.Message}",
                Namespaces = []
            };
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveDocumentationAssembly;
        }
    }

    private void RememberDirectory(string directory)
    {
        if (!string.IsNullOrEmpty(directory) && !documentationDirectories.Contains(directory))
        {
            documentationDirectories.Add(directory);
        }
    }

    private Assembly ResolveDocumentationAssembly(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        if (string.IsNullOrEmpty(assemblyName.Name) || !assembliesBeingResolved.Add(assemblyName.Name))
        {
            return null;
        }

        try
        {
            return ResolveDocumentationAssemblyCore(sender, args, assemblyName);
        }
        finally
        {
            assembliesBeingResolved.Remove(assemblyName.Name);
        }
    }

    private Assembly ResolveDocumentationAssemblyCore(object sender, ResolveEventArgs args, AssemblyName assemblyName)
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(candidate => candidate.GetName().Name == assemblyName.Name);
        if (loaded != null)
        {
            return loaded;
        }

        foreach (string directory in documentationDirectories)
        {
            string dllPath = Path.Combine(directory, assemblyName.Name + ".dll");
            if (File.Exists(dllPath))
            {
                return Assembly.LoadFrom(dllPath);
            }
        }

        return ResolveFrameworkAssembly(sender, args);
    }

    private static Guid StableGuid(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
        byte[] bytes = new byte[16];
        Array.Copy(hash, bytes, 16);
        return new Guid(bytes);
    }

    private static IEnumerable<Type> GetTypesMetadataOnly(string assemblyPath)
    {
        try
        {
            // Create resolver with core framework paths
            var resolver = new PathAssemblyResolver(
            [
                typeof(object).Assembly.Location,
                Path.GetDirectoryName(typeof(object).Assembly.Location),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared"),
                assemblyPath
            ]);

            using var mlc = new MetadataLoadContext(resolver);
            var assembly = mlc.LoadFromAssemblyPath(assemblyPath);
            return assembly.GetTypes();
        }
        catch
        {
            return [];
        }
    }

    private static Assembly ResolveFrameworkAssembly(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        // Check already loaded assemblies first
        var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);

        if (loadedAssembly != null)
        {
            return loadedAssembly;
        }

        // Search framework directories
        foreach (string frameworkPath in FrameworkPaths)
        {
            if (!Directory.Exists(frameworkPath))
            {
                continue;
            }

            // Get the highest version available
            string versionDir = Directory.GetDirectories(frameworkPath)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            if (versionDir != null)
            {
                string dllPath = Path.Combine(versionDir, $"{assemblyName.Name}.dll");
                if (File.Exists(dllPath))
                {
                    return Assembly.LoadFrom(dllPath);
                }
            }
        }

        return null;
    }

    private void FindTypes(Assembly assembly, ICollection<NamespaceModel> namespaces)
    {
        var types = GetLoadableTypes(assembly)
            .Where(x => x.IsVisible && !IsCompilerGenerated(x))
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
                var commentsElement = ResolveComments(type);
                var comments = GetCommonTags(commentsElement);

                var typeBase = new TypeModel
                {
                    Namespace = @namespace,
                    IsAbstract = type.IsAbstract,
                    IsPrimitive = type.IsPrimitive,
                    IsPublic = type.IsPublic || type.IsNestedPublic,
                    IsSealed = type.IsSealed,
                    IsNested = type.IsNested,
                    ParentClass = type.BaseType != null ? type.BaseType.FullName : string.Empty,
                    Parents = GetParents(type),
                    Interfaces = GetInterfaces(type),
                    Name = FormatType(type, fullName: false),
                    FullName = FormatType(type, fullName: true),
                    Example = comments.Example,
                    Exceptions = comments.Exceptions,
                    Remarks = comments.Remarks,
                    Returns = comments.Returns,
                    Summary = comments.Summary
                };

                typeBase.Constructors = GetConstructors(type, typeBase);
                typeBase.Methods = GetMethods(type, typeBase);
                typeBase.Properties = GetProperties(type, typeBase);
                typeBase.Events = GetEvents(type);
                typeBase.Fields = GetFields(type);

                if (type.IsEnum)
                {
                    typeBase.ObjectType = ObjectType.Enumeration;
                    typeBase.Members = GetMembers(type);
                    @namespace.Enumerations.Add(typeBase);
                }
                else if (typeof(Delegate).IsAssignableFrom(type))
                {
                    typeBase.ObjectType = ObjectType.Delegate;
                    @namespace.Delegates.Add(typeBase);
                }
                else if (type.IsInterface)
                {
                    typeBase.ObjectType = ObjectType.Interface;
                    @namespace.Interfaces.Add(typeBase);
                }
                else if (type.IsValueType)
                {
                    typeBase.ObjectType = ObjectType.Structure;
                    @namespace.Structures.Add(typeBase);
                }
                else if (type.IsClass)
                {
                    typeBase.ObjectType = ObjectType.Class;
                    @namespace.Classes.Add(typeBase);
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
            comments.Example = DocCommentText.RenderChild(commentsElement, "example");
            comments.Remarks = JoinComments(
                DocCommentText.RenderChild(commentsElement, "remarks"),
                DocCommentText.RenderTypeParameters(commentsElement),
                DocCommentText.RenderSeeAlso(commentsElement));
            comments.Returns = DocCommentText.RenderChild(commentsElement, "returns");
            comments.Summary = DocCommentText.RenderChild(commentsElement, "summary");
            comments.Exceptions = DocCommentText.RenderExceptions(commentsElement);

            string value = DocCommentText.RenderChild(commentsElement, "value");
            if (string.IsNullOrEmpty(comments.Summary))
            {
                comments.Summary = value;
            }
            else if (!string.IsNullOrEmpty(value))
            {
                comments.Remarks = JoinComments(comments.Remarks, value);
            }
        }

        return comments;
    }

    private static string JoinComments(params string[] parts)
    {
        string[] present = parts.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
        return present.Length == 0 ? string.Empty : string.Join("\n\n", present);
    }

    private ICollection<ConstructorModel> GetConstructors(Type type, TypeModel parent)
    {
        var list = new List<ConstructorModel>();

        foreach (var constructorInfo in type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            try
            {
                if (!IsDocumented(constructorInfo))
                {
                    continue;
                }
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
                    constructor.FullName = ReflectionHelper.GetConstructorSignature(constructorInfo);

                    var commentsElement = ResolveComments(constructorInfo);
                    var comments = GetCommonTags(commentsElement);
                    constructor.Example = comments.Example;
                    constructor.Exceptions = comments.Exceptions;
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

        list = list.OrderBy(m => m.Parameters?.Count ?? 0).ThenBy(m => m.FullName).ToList();
        return list;
    }

    private ICollection<MemberSummaryModel> GetMembers(Type type)
    {
        // For enumerations
        var list = (from info in type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                    where info.Name != "value__"
                    let element = ResolveComments(info)
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
        IEnumerable<XElement> paramElements = [];

        if (element != null)
        {
            paramElements = element.Elements().Where(e => e.Name.LocalName == "param");
        }

        var list = new List<ParameterModel>();
        var parameterInfos = parameters as ParameterInfo[] ?? parameters.ToArray();
        var paramDocs = paramElements as XElement[] ?? paramElements.ToArray();
        bool anyNameMatches = parameterInfos.Any(info => paramDocs.Any(doc => doc.Attribute("name")?.Value == info.Name));
        bool isExtension = parameterInfos.FirstOrDefault()?.Member.IsDefined(typeof(ExtensionAttribute), false) == true;
        for (int index = 0; index < parameterInfos.Length; index++)
        {
            var parameterInfo = parameterInfos[index];
            var parameter = new ParameterModel
            {
                Name = parameterInfo.Name,
                IsOut = parameterInfo.IsOut,
                IsRetval = parameterInfo.IsRetval,
                Type = ReflectionHelper.FormatParameter(parameterInfo, fullName: false, isExtensionParameter: isExtension && index == 0),
                TypeFullName = ReflectionHelper.FormatParameter(parameterInfo, fullName: true, isExtensionParameter: isExtension && index == 0)
            };

            var current = anyNameMatches
                ? paramDocs.FirstOrDefault(doc => doc.Attribute("name")?.Value == parameter.Name)
                : index < paramDocs.Length ? paramDocs[index] : null;
            if (current != null)
            {
                parameter.Description = DocCommentText.RenderElement(current);
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
        foreach (var methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            try
            {
                bool isOperator = methodInfo.IsSpecialName && methodInfo.Name.StartsWith("op_", StringComparison.Ordinal);
                if ((methodInfo.IsSpecialName && !isOperator) || !IsDocumented(methodInfo))
                {
                    continue;
                }

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
                method.Name = ReflectionHelper.FormatMethodName(methodInfo);

                if (methodInfo.IsGenericMethod || isOperator)
                {
                    method.UseHashCodeForId = true;
                }

                try
                {
                    method.FullName = ReflectionHelper.GetMethodSignature(methodInfo);
                    method.ReturnType = ReflectionHelper.FormatReturnType(methodInfo);
                    method.ReturnTypeFullName = FormatType(methodInfo.ReturnType, fullName: true);

                    var element = ResolveComments(methodInfo);
                    var comments = GetCommonTags(element);
                    method.Example = comments.Example;
                    method.Exceptions = comments.Exceptions;
                    method.Remarks = comments.Remarks;
                    method.Returns = comments.Returns;
                    method.Summary = comments.Summary;
                    method.Parameters = GetMethodParameters(methodInfo.GetParameters(), element);
                }
                catch (Exception x)
                {
                    method.LoadError = true;
                    method.FullName = x.Message;
                }

                list.Add(method);
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
                Name = FormatType(current.BaseType, fullName: false),
                FullName = FormatType(current.BaseType, fullName: true)
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
                var getter = propertyInfo.GetGetMethod(true);
                var setter = propertyInfo.GetSetMethod(true);
                if (!IsDocumented(getter) && !IsDocumented(setter))
                {
                    continue;
                }

                var property = new PropertyModel
                {
                    Name = propertyInfo.GetIndexParameters().Length > 0 ? "this[]" : propertyInfo.Name,
                    Parent = parent,
                    ParentClass = parent.Id,
                    Attributes = propertyInfo.Attributes.ToString(),
                };

                try
                {
                    property.FullName = ReflectionHelper.GetPropertySignature(propertyInfo);
                    property.Type = FormatType(propertyInfo.PropertyType, fullName: false);
                    property.TypeFullName = FormatType(propertyInfo.PropertyType, fullName: true);

                    var commentsElement = ResolveComments(propertyInfo);
                    var comments = GetCommonTags(commentsElement);
                    property.Example = comments.Example;
                    property.Exceptions = comments.Exceptions;
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

    private static string FormatType(Type type, bool fullName) => ReflectionHelper.FormatType(type, fullName);

    private static bool IsCompilerGenerated(Type type) =>
        type.Name.StartsWith('<') || type.IsDefined(typeof(CompilerGeneratedAttribute), false);

    private static bool IsDocumented(MethodBase method) => method != null && (!method.IsConstructor || !method.IsStatic) && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);

    private void RegisterDocumentation(string assemblyFile)
    {
        string assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
        if (string.IsNullOrEmpty(assemblyName) || commentReaders.ContainsKey(assemblyName) || missingDocumentation.Contains(assemblyName))
        {
            return;
        }

        var reader = OpenDocumentation(assemblyFile);
        if (reader == null)
        {
            missingDocumentation.Add(assemblyName);
            return;
        }

        commentReaders[assemblyName] = reader;
    }

    private static XmlDocCommentReader OpenDocumentation(string assemblyFile)
    {
        string xmlFile = Path.ChangeExtension(assemblyFile, ".xml");
        if (!File.Exists(xmlFile))
        {
            return null;
        }

        try
        {
            return new XmlDocCommentReader(xmlFile);
        }
        catch (Exception x)
        {
            Trace.TraceError($"XML {xmlFile} : {x.Message} => {x.Source}");
            return null;
        }
    }

    private XElement ResolveComments(MemberInfo member)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        return ResolveComments(member, visited);
    }

    private XElement ResolveComments(MemberInfo member, HashSet<string> visited)
    {
        if (member == null)
        {
            return null;
        }

        member = DocumentationMember(member);
        string key = member.Module.ModuleVersionId + ":" + member.MetadataToken;
        if (!visited.Add(key))
        {
            return null;
        }

        var element = ReadComments(member);
        if (!DocCommentText.IsInheritDoc(element))
        {
            return element;
        }

        string cref = element.Elements()
            .FirstOrDefault(item => item.Name.LocalName == "inheritdoc")
            ?.Attribute("cref")?.Value;
        if (!string.IsNullOrEmpty(cref))
        {
            var targeted = ReadMember(cref);
            if (HasDocumentation(targeted))
            {
                return targeted;
            }
        }

        foreach (var source in DocumentationSources(member))
        {
            var inherited = ResolveComments(source, visited);
            if (HasDocumentation(inherited))
            {
                return inherited;
            }
        }

        return element;
    }

    private XElement ReadComments(MemberInfo member)
    {
        var reader = ReaderFor(member);
        if (reader == null || member == null)
        {
            return null;
        }

        try
        {
            return member switch
            {
                Type type => reader.GetComments(type),
                ConstructorInfo constructor => reader.GetComments(constructor),
                MethodInfo method => reader.GetComments(method),
                PropertyInfo property => reader.GetComments(property),
                EventInfo eventInfo => reader.GetComments(eventInfo),
                FieldInfo field => reader.GetComments(field),
                _ => reader.GetComments(member)
            };
        }
        catch (Exception x)
        {
            Trace.TraceError($"Comments for {member.Name} : {x.Message}");
            return null;
        }
    }

    private XElement ReadMember(string memberName)
    {
        if (xmlDocCommentReader != null)
        {
            var current = xmlDocCommentReader.ReadPolicy.ReadMember(memberName);
            if (current != null)
            {
                return current;
            }
        }

        foreach (var reader in commentReaders.Values)
        {
            if (reader == null || reader == xmlDocCommentReader)
            {
                continue;
            }

            var found = reader.ReadPolicy.ReadMember(memberName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private XmlDocCommentReader ReaderFor(MemberInfo member)
    {
        var assembly = member is Type type ? type.Assembly : member.DeclaringType?.Assembly ?? member.Module?.Assembly;
        if (assembly == null)
        {
            return xmlDocCommentReader;
        }

        string name = assembly.GetName().Name;
        if (string.IsNullOrEmpty(name))
        {
            return xmlDocCommentReader;
        }

        if (commentReaders.TryGetValue(name, out var cached))
        {
            return cached;
        }

        if (missingDocumentation.Contains(name))
        {
            return null;
        }

        string xmlPath = null;
        if (!string.IsNullOrEmpty(assembly.Location))
        {
            string besideAssembly = Path.ChangeExtension(assembly.Location, ".xml");
            if (File.Exists(besideAssembly))
            {
                xmlPath = besideAssembly;
            }
        }

        if (xmlPath == null && !string.IsNullOrEmpty(sourceDirectory))
        {
            string besideSource = Path.Combine(sourceDirectory, name + ".xml");
            if (File.Exists(besideSource))
            {
                xmlPath = besideSource;
            }
        }

        if (xmlPath == null)
        {
            if (xmlDocCommentReader != null
                && string.Equals(Path.GetFileNameWithoutExtension(xmlDocCommentReader.FullPath), name, StringComparison.OrdinalIgnoreCase))
            {
                commentReaders[name] = xmlDocCommentReader;
                return xmlDocCommentReader;
            }

            missingDocumentation.Add(name);
            return null;
        }

        try
        {
            var reader = new XmlDocCommentReader(xmlPath);
            commentReaders[name] = reader;
            return reader;
        }
        catch (Exception x)
        {
            Trace.TraceError($"XML {xmlPath} : {x.Message} => {x.Source}");
            missingDocumentation.Add(name);
            return null;
        }
    }

    private static bool HasDocumentation(XElement element) =>
        element != null && !DocCommentText.IsInheritDoc(element) && element.Elements().Any();

    private static MemberInfo DocumentationMember(MemberInfo member) =>
        member is MethodInfo { IsGenericMethod: true } method ? method.GetGenericMethodDefinition() : member;

    private static IEnumerable<MemberInfo> DocumentationSources(MemberInfo member)
    {
        switch (member)
        {
            case MethodInfo method:
                var basis = BaseMethod(method);
                if (basis != null)
                {
                    yield return basis;
                }

                foreach (var interfaceMethod in ImplementedInterfaceMethods(method))
                {
                    yield return interfaceMethod;
                }

                break;

            case PropertyInfo property:
                var baseProperty = BaseProperty(property);
                if (baseProperty != null)
                {
                    yield return baseProperty;
                }

                foreach (var interfaceProperty in ImplementedInterfaceProperties(property))
                {
                    yield return interfaceProperty;
                }

                break;

            case EventInfo eventInfo:
                var baseEvent = BaseEvent(eventInfo);
                if (baseEvent != null)
                {
                    yield return baseEvent;
                }

                foreach (var interfaceEvent in ImplementedInterfaceEvents(eventInfo))
                {
                    yield return interfaceEvent;
                }

                break;

            case Type type when type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType):
                yield return type.BaseType;
                foreach (var interfaceType in DirectInterfaces(type))
                {
                    yield return interfaceType;
                }

                break;

            case Type type:
                foreach (var interfaceType in DirectInterfaces(type))
                {
                    yield return interfaceType;
                }

                break;
        }
    }

    private static MethodInfo BaseMethod(MethodInfo method)
    {
        try
        {
            var basis = method.GetBaseDefinition();
            return basis == null || SameMethod(basis, method) ? null : basis.IsGenericMethod ? basis.GetGenericMethodDefinition() : basis;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static PropertyInfo BaseProperty(PropertyInfo property)
    {
        var accessor = property.GetGetMethod(true) ?? property.GetSetMethod(true);
        var basis = accessor == null ? null : BaseMethod(accessor);
        return basis == null ? null : FindProperty(basis);
    }

    private static EventInfo BaseEvent(EventInfo eventInfo)
    {
        var add = eventInfo.GetAddMethod(true);
        var basis = add == null ? null : BaseMethod(add);
        return basis == null ? null : FindEvent(basis);
    }

    private static IEnumerable<MethodInfo> ImplementedInterfaceMethods(MethodInfo method)
    {
        var type = method.DeclaringType;
        if (type == null || type.IsInterface)
        {
            yield break;
        }

        bool mapped = false;
        foreach (var interfaceType in SafeInterfaces(type))
        {
            InterfaceMapping map;
            try
            {
                map = type.GetInterfaceMap(interfaceType);
            }
            catch (Exception)
            {
                continue;
            }

            for (int index = 0; index < map.TargetMethods.Length; index++)
            {
                if (!SameMethod(map.TargetMethods[index], method))
                {
                    continue;
                }

                mapped = true;
                var interfaceMethod = map.InterfaceMethods[index];
                yield return interfaceMethod.IsGenericMethod ? interfaceMethod.GetGenericMethodDefinition() : interfaceMethod;
            }
        }

        if (mapped)
        {
            yield break;
        }

        foreach (var interfaceType in SafeInterfaces(type))
        {
            MethodInfo[] methods;
            try
            {
                methods = interfaceType.GetMethods();
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var candidate in methods)
            {
                if (SignatureMatches(method, candidate))
                {
                    yield return candidate.IsGenericMethod ? candidate.GetGenericMethodDefinition() : candidate;
                }
            }
        }
    }

    private static IEnumerable<PropertyInfo> ImplementedInterfaceProperties(PropertyInfo property)
    {
        var accessor = property.GetGetMethod(true) ?? property.GetSetMethod(true);
        if (accessor == null)
        {
            yield break;
        }

        foreach (var interfaceAccessor in ImplementedInterfaceMethods(accessor))
        {
            var match = FindProperty(interfaceAccessor);
            if (match != null)
            {
                yield return match;
            }
        }
    }

    private static IEnumerable<EventInfo> ImplementedInterfaceEvents(EventInfo eventInfo)
    {
        var add = eventInfo.GetAddMethod(true);
        if (add == null)
        {
            yield break;
        }

        foreach (var interfaceAdd in ImplementedInterfaceMethods(add))
        {
            var match = FindEvent(interfaceAdd);
            if (match != null)
            {
                yield return match;
            }
        }
    }

    private static IEnumerable<Type> SafeInterfaces(Type type)
    {
        try
        {
            return type.GetInterfaces();
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static IEnumerable<Type> DirectInterfaces(Type type)
    {
        try
        {
            var inherited = type.BaseType?.GetInterfaces() ?? Type.EmptyTypes;
            return type.GetInterfaces().Except(inherited);
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static PropertyInfo FindProperty(MethodInfo accessor)
    {
        if (accessor?.DeclaringType == null)
        {
            return null;
        }

        var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        return accessor.DeclaringType.GetProperties(flags)
            .FirstOrDefault(candidate => SameMethod(candidate.GetGetMethod(true), accessor) || SameMethod(candidate.GetSetMethod(true), accessor));
    }

    private static EventInfo FindEvent(MethodInfo accessor)
    {
        if (accessor?.DeclaringType == null)
        {
            return null;
        }

        var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        return accessor.DeclaringType.GetEvents(flags)
            .FirstOrDefault(candidate => SameMethod(candidate.GetAddMethod(true), accessor) || SameMethod(candidate.GetRemoveMethod(true), accessor));
    }

    private static bool SameMethod(MethodInfo left, MethodInfo right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        if (ReferenceEquals(left, right))
        {
            return true;
        }

        var leftDefinition = left.IsGenericMethod ? left.GetGenericMethodDefinition() : left;
        var rightDefinition = right.IsGenericMethod ? right.GetGenericMethodDefinition() : right;
        return leftDefinition.MetadataToken == rightDefinition.MetadataToken && leftDefinition.Module == rightDefinition.Module;
    }

    private static bool SignatureMatches(MethodInfo implementation, MethodInfo contract)
    {
        string name = implementation.Name;
        if (!string.Equals(name, contract.Name, StringComparison.Ordinal)
            && !name.EndsWith("." + contract.Name, StringComparison.Ordinal))
        {
            return false;
        }

        var left = implementation.GetParameters();
        var right = contract.GetParameters();
        if (left.Length != right.Length)
        {
            return false;
        }

        for (int index = 0; index < left.Length; index++)
        {
            if (!string.Equals(TypeKey(left[index].ParameterType), TypeKey(right[index].ParameterType), StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string TypeKey(Type type)
    {
        if (type == null)
        {
            return string.Empty;
        }

        if (type.IsByRef)
        {
            return TypeKey(type.GetElementType()) + "&";
        }

        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsGenericType)
        {
            string name = type.Name;
            int tick = name.IndexOf('`');
            if (tick >= 0)
            {
                name = name[..tick];
            }

            string arguments = string.Join(",", type.GetGenericArguments().Select(TypeKey));
            return name + "<" + arguments + ">";
        }

        return type.FullName ?? type.Name;
    }

    private ICollection<string> GetInterfaces(Type type)
    {
        try
        {
            var inherited = type.BaseType?.GetInterfaces() ?? Type.EmptyTypes;
            return type.GetInterfaces()
                .Except(inherited)
                .Select(implemented => FormatType(implemented, fullName: true))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
        }
        catch (Exception)
        {
            return [];
        }
    }

    private ICollection<EventModel> GetEvents(Type type)
    {
        var list = new List<EventModel>();
        foreach (var eventInfo in type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            try
            {
                if (!IsDocumented(eventInfo.GetAddMethod(true)))
                {
                    continue;
                }

                var model = new EventModel
                {
                    Name = eventInfo.Name,
                    Type = FormatType(eventInfo.EventHandlerType, fullName: false),
                    TypeFullName = FormatType(eventInfo.EventHandlerType, fullName: true),
                    FullName = $"{FormatType(eventInfo.EventHandlerType, fullName: true)} {eventInfo.Name}"
                };

                var comments = GetCommonTags(ResolveComments(eventInfo));
                model.Example = comments.Example;
                model.Exceptions = comments.Exceptions;
                model.Remarks = comments.Remarks;
                model.Summary = comments.Summary;
                list.Add(model);
            }
            catch (Exception x)
            {
                Trace.TraceError($"Event {eventInfo.Name} : Parse Problem. {x.Message} => {x.Source}");
            }
        }

        return list.OrderBy(item => item.Name).ToList();
    }

    private ICollection<FieldModel> GetFields(Type type)
    {
        if (type.IsEnum)
        {
            return [];
        }

        var list = new List<FieldModel>();
        foreach (var field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        {
            try
            {
                if (field.IsSpecialName || field.Name.Contains('<'))
                {
                    continue;
                }

                var comments = GetCommonTags(ResolveComments(field));
                list.Add(new FieldModel
                {
                    Name = field.Name,
                    IsConst = field.IsLiteral && !field.IsInitOnly,
                    IsStatic = field.IsStatic,
                    Type = FormatType(field.FieldType, fullName: false),
                    TypeFullName = FormatType(field.FieldType, fullName: true),
                    FullName = $"{FormatType(field.FieldType, fullName: true)} {field.Name}",
                    Summary = comments.Summary,
                    Remarks = comments.Remarks,
                    Exceptions = comments.Exceptions,
                    Example = comments.Example
                });
            }
            catch (Exception x)
            {
                Trace.TraceError($"Field {field.Name} : Parse Problem. {x.Message} => {x.Source}");
            }
        }

        return list.OrderBy(item => item.Name).ToList();
    }
}