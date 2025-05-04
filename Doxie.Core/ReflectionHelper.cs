using System.Reflection;

namespace Doxie.Core;

public static class ReflectionHelper
{
    public static string GetMethodSignature(MethodInfo methodInfo)
    {
        string methodName = methodInfo.Name;
        string[] parameters = methodInfo.GetParameters()
            .Select(p => p.ParameterType.Name)
            .ToArray();

        return $"{methodName}({string.Join(", ", parameters)})";
    }

    public static string GetPropertySignature(PropertyInfo propertyInfo)
    {
        string propertyName = propertyInfo.Name;
        string propertyType = propertyInfo.PropertyType.Name;
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