using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Doxie.Core;

public static class ReflectionHelper
{
    public static string GetMethodSignature(MethodInfo methodInfo)
    {
        // Return type
        var returnType = GetTypeName(methodInfo.ReturnType);

        // Method name
        var methodName = methodInfo.Name;

        // Parameters (with params/array/generic support)
        var parameters = methodInfo.GetParameters()
            .Select(p => GetParameterTypeName(p.ParameterType, p.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any()))
            .ToArray();

        return $"{returnType} {methodName}({string.Join(", ", parameters)})";
    }

    private static string GetParameterTypeName(Type type, bool isParams)
    {
        var typeName = GetTypeName(type);

        if (isParams)
        {
            // Handle params arrays (e.g., "Expression<Func<T, dynamic>>[]" instead of "Expression`1[]")
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return $"{GetTypeName(elementType)}[]";
            }
        }
        return typeName;
    }

    private static string GetTypeName(Type type)
    {
        // Handle dynamic
        if (type == typeof(object) && type.IsDefined(typeof(DynamicAttribute), inherit: true))
            return "dynamic";

        // Handle nullable (e.g., int?)
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            return $"{GetTypeName(underlyingType)}?";

        // Handle arrays (non-params cases)
        if (type.IsArray)
            return $"{GetTypeName(type.GetElementType())}[]";

        // Handle non-generic types
        if (!type.IsGenericType)
            return GetSimpleTypeName(type);

        // Handle generic types
        var genericTypeDef = type.GetGenericTypeDefinition();
        var genericTypeName = GetSimpleTypeName(genericTypeDef).Split('`')[0];
        var genericArgs = type.GetGenericArguments().Select(GetTypeName).ToArray();

        return $"{genericTypeName}<{string.Join(", ", genericArgs)}>";
    }

    private static string GetSimpleTypeName(Type type)
    {
        // Convert common framework type names to C# aliases
        return Type.GetTypeCode(type) switch
        {
            //TypeCode.Boolean => "bool",
            //TypeCode.Int32 => "int",
            //TypeCode.String => "string",
            //TypeCode.Object => type.FullName ?? type.Name,
            _ => type.Name
        };
    }

    public static string GetPropertySignature(PropertyInfo propertyInfo)
    {
        string propertyName = propertyInfo.Name;
        string propertyType = propertyInfo.PropertyType.FullName ?? propertyInfo.PropertyType.Name;

        return $"{propertyType} {propertyName}";
    }

    public static string GetConstructorSignature(ConstructorInfo constructorInfo)
    {
        string declaringType = constructorInfo.ReflectedType?.Name ?? constructorInfo.DeclaringType?.Name;
        string[] parameters = constructorInfo.GetParameters()
            .Select(p => p.ParameterType.Name)
            .ToArray();

        return $"{declaringType}({string.Join(", ", parameters)})";
    }
}