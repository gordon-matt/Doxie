using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Doxie.Core;

public static class ReflectionHelper
{
    private static readonly Dictionary<Type, string> Aliases = new()
    {
        [typeof(void)] = "void",
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(char)] = "char",
        [typeof(decimal)] = "decimal",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(object)] = "object",
        [typeof(string)] = "string"
    };

    private static readonly Dictionary<string, string> OperatorSymbols = new(StringComparer.Ordinal)
    {
        ["op_Addition"] = "+",
        ["op_Subtraction"] = "-",
        ["op_Multiply"] = "*",
        ["op_Division"] = "/",
        ["op_Modulus"] = "%",
        ["op_ExclusiveOr"] = "^",
        ["op_BitwiseAnd"] = "&",
        ["op_BitwiseOr"] = "|",
        ["op_LogicalAnd"] = "&&",
        ["op_LogicalOr"] = "||",
        ["op_LeftShift"] = "<<",
        ["op_RightShift"] = ">>",
        ["op_UnsignedRightShift"] = ">>>",
        ["op_Equality"] = "==",
        ["op_Inequality"] = "!=",
        ["op_GreaterThan"] = ">",
        ["op_LessThan"] = "<",
        ["op_GreaterThanOrEqual"] = ">=",
        ["op_LessThanOrEqual"] = "<=",
        ["op_UnaryPlus"] = "+",
        ["op_UnaryNegation"] = "-",
        ["op_Increment"] = "++",
        ["op_Decrement"] = "--",
        ["op_OnesComplement"] = "~",
        ["op_LogicalNot"] = "!",
        ["op_True"] = "true",
        ["op_False"] = "false",
        ["op_Implicit"] = "implicit",
        ["op_Explicit"] = "explicit"
    };

    public static string GetMethodSignature(MethodInfo methodInfo)
    {
        string returnType = FormatReturnType(methodInfo);
        string methodName = FormatMethodName(methodInfo);
        bool isExtension = methodInfo.IsDefined(typeof(ExtensionAttribute), false);
        string parameters = string.Join(", ", methodInfo.GetParameters().Select((parameter, index) =>
            FormatParameter(parameter, fullName: false, isExtensionParameter: isExtension && index == 0)));

        return $"{returnType} {methodName}({parameters})";
    }

    public static string GetPropertySignature(PropertyInfo propertyInfo)
    {
        string propertyType = FormatType(propertyInfo.PropertyType, fullName: true);
        if (propertyInfo.GetIndexParameters().Length > 0)
        {
            string indexes = string.Join(", ", propertyInfo.GetIndexParameters().Select(parameter => FormatParameter(parameter, fullName: true, isExtensionParameter: false)));
            return $"{propertyType} this[{indexes}]";
        }

        return $"{propertyType} {propertyInfo.Name}";
    }

    public static string GetConstructorSignature(ConstructorInfo constructorInfo)
    {
        string name = constructorInfo.DeclaringType == null
            ? "ctor"
            : FormatType(constructorInfo.DeclaringType, fullName: false);
        int tick = name.IndexOf('`');
        if (tick >= 0)
        {
            name = name[..tick];
        }

        string parameters = string.Join(", ", constructorInfo.GetParameters().Select(parameter => FormatParameter(parameter, fullName: false, isExtensionParameter: false)));
        return $"{name}({parameters})";
    }

    public static string FormatMethodName(MethodInfo method)
    {
        if (method.IsSpecialName && method.Name.StartsWith("op_", StringComparison.Ordinal))
        {
            return !OperatorSymbols.TryGetValue(method.Name, out string symbol)
                ? method.Name
                : method.Name is "op_Implicit" or "op_Explicit"
                ? $"{symbol} operator"
                : $"operator {symbol}";
        }

        if (method.IsGenericMethod)
        {
            string arguments = string.Join(", ", method.GetGenericArguments().Select(type => type.Name));
            return $"{method.Name}<{arguments}>";
        }

        return method.Name;
    }

    public static string FormatReturnType(MethodInfo method)
    {
        if (IsDynamic(method.ReturnParameter) && method.ReturnType == typeof(object))
        {
            return "dynamic";
        }

        string name = FormatType(method.ReturnType, fullName: false);
        return IsNullableReference(method.ReturnParameter) ? name + "?" : name;
    }

    public static string FormatParameter(ParameterInfo parameter, bool fullName, bool isExtensionParameter)
    {
        var type = parameter.ParameterType;
        string modifier = string.Empty;
        if (type.IsByRef)
        {
            modifier = parameter.IsOut ? "out " : parameter.IsIn ? "in " : "ref ";
            type = type.GetElementType();
        }

        string typeName = IsDynamic(parameter) && type == typeof(object)
            ? "dynamic"
            : FormatType(type, fullName);

        if (typeName != "dynamic" && IsNullableReference(parameter))
        {
            typeName += "?";
        }

        if (parameter.IsDefined(typeof(ParamArrayAttribute), false))
        {
            modifier = "params " + modifier;
        }

        if (isExtensionParameter)
        {
            modifier = "this " + modifier;
        }

        string formatted = modifier + typeName;
        if (fullName && TryGetDefaultValue(parameter, out string defaultValue))
        {
            formatted += " = " + defaultValue;
        }

        return formatted;
    }

    public static string FormatType(Type type, bool fullName)
    {
        if (type == null)
        {
            return string.Empty;
        }

        if (type.IsByRef)
        {
            return FormatType(type.GetElementType(), fullName);
        }

        if (type.IsPointer)
        {
            return FormatType(type.GetElementType(), fullName) + "*";
        }

        if (type.IsArray)
        {
            int rank = type.GetArrayRank();
            string suffix = rank == 1 ? "[]" : "[" + new string(',', rank - 1) + "]";
            return FormatType(type.GetElementType(), fullName) + suffix;
        }

        var nullable = Nullable.GetUnderlyingType(type);
        if (nullable != null)
        {
            return FormatType(nullable, fullName) + "?";
        }

        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (Aliases.TryGetValue(type, out string alias))
        {
            return alias;
        }

        if (type.IsGenericType)
        {
            string name = GenericTypeName(type, fullName);
            string arguments = string.Join(", ", type.GetGenericArguments().Select(argument => FormatType(argument, fullName)));
            return $"{name}<{arguments}>";
        }

        return type.IsNested && type.DeclaringType != null
            ? FormatType(type.DeclaringType, fullName) + "." + type.Name
            : fullName && !string.IsNullOrEmpty(type.FullName) ? type.FullName : type.Name;
    }

    private static string GenericTypeName(Type type, bool fullName)
    {
        string rawName = type.Name;
        int tick = rawName.IndexOf('`');
        string name = tick >= 0 ? rawName[..tick] : rawName;

        return type.IsNested && type.DeclaringType != null
            ? FormatType(type.DeclaringType, fullName) + "." + name
            : !fullName || string.IsNullOrEmpty(type.Namespace) ? name : type.Namespace + "." + name;
    }

    private static bool IsDynamic(ParameterInfo parameter) =>
        parameter != null && parameter.IsDefined(typeof(DynamicAttribute), false);

    private static bool IsNullableReference(ParameterInfo parameter)
    {
        if (parameter == null || parameter.ParameterType.IsValueType || (parameter.ParameterType.IsByRef && parameter.ParameterType.GetElementType().IsValueType))
        {
            return false;
        }

        var attribute = parameter.GetCustomAttributesData()
            .FirstOrDefault(item => item.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

        if (attribute == null || attribute.ConstructorArguments.Count == 0)
        {
            return false;
        }

        object value = attribute.ConstructorArguments[0].Value;
        return value is byte flag
            ? flag == 2
            : value is IList<CustomAttributeTypedArgument> flags && flags.Count > 0 && flags[0].Value is byte first && first == 2;
    }

    private static bool TryGetDefaultValue(ParameterInfo parameter, out string value)
    {
        value = null;
        try
        {
            if (!parameter.HasDefaultValue)
            {
                return false;
            }

            value = FormatLiteral(parameter.DefaultValue);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string FormatLiteral(object value) => value switch
    {
        null => "null",
        string text => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
        char character => $"'{character}'",
        bool boolean => boolean ? "true" : "false",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null",
    };
}