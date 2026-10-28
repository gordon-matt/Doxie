using Doxie.Core.Tests.Samples;
using Doxie.Core.XmlComments;

namespace Doxie.Core.Tests;

public class DocParserTests
{
    [Fact]
    public void Parse_DocumentedSample_ImportsCommentsEventsAndFields()
    {
        var parser = new DocParser();
        var assembly = parser.Parse(typeof(DocumentedSample).Assembly.Location);

        var type = assembly.Namespaces
            .SelectMany(item => item.Classes)
            .Single(item => item.Name == nameof(DocumentedSample));

        Assert.Contains("Doxie.Core.Tests.Samples.ISample", type.Interfaces);
        Assert.Contains("DocumentedSample.Limit", type.Summary);
        Assert.Contains(type.Fields, field => field.Name == nameof(DocumentedSample.Limit) && field.IsConst);
        Assert.Contains(type.Events, item => item.Name == nameof(DocumentedSample.Changed));

        var echo = type.Methods.Single(method => method.Name == nameof(DocumentedSample.Echo));
        Assert.Contains("incoming", echo.Summary);
        Assert.Equal("The incoming value.", echo.Parameters.Single().Description);
        Assert.Contains("ArgumentNullException", echo.Exceptions);
        Assert.Equal("The current title.", type.Properties.Single(property => property.Name == nameof(DocumentedSample.Label)).Summary);

        var finder = assembly.Namespaces
            .SelectMany(item => item.Classes)
            .Single(item => item.Name == "Finder<T>");
        Assert.Contains("id", finder.Methods.Single(method => method.Name == "Find").Summary);

        var take = type.Methods.Single(method => method.Name == nameof(DocumentedSample.Take));
        Assert.Equal("params int[]", take.Parameters.Single().Type);
    }
}