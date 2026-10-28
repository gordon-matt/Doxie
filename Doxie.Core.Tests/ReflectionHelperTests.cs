using Doxie.Core.Tests.Samples;

namespace Doxie.Core.Tests;

public class ReflectionHelperTests
{
    [Fact]
    public void FormatType_ClosedGeneric_UsesAliases()
    {
        Assert.Equal("List<int>", ReflectionHelper.FormatType(typeof(List<int>), fullName: false));
        Assert.Equal("Dictionary<string, int>", ReflectionHelper.FormatType(typeof(Dictionary<string, int>), fullName: false));
    }

    [Fact]
    public void FormatType_NullableAndArray_KeepsSuffix()
    {
        Assert.Equal("int?", ReflectionHelper.FormatType(typeof(int?), fullName: false));
        Assert.Equal("string[]", ReflectionHelper.FormatType(typeof(string[]), fullName: false));
    }

    [Fact]
    public void FormatParameter_ParamsArray_IncludesModifier()
    {
        var parameter = typeof(DocumentedSample).GetMethod(nameof(DocumentedSample.Take))!.GetParameters()[0];

        Assert.Equal("params int[]", ReflectionHelper.FormatParameter(parameter, fullName: false, isExtensionParameter: false));
    }
}