using System.Xml.Linq;
using Doxie.Core.XmlComments;

namespace Doxie.Core.Tests;

public class DocCommentTextTests
{
    [Fact]
    public void RenderElement_SeeAndParamRef_KeepsNames()
    {
        var summary = XElement.Parse("<summary>Stores <paramref name=\"incoming\"/>. See <see cref=\"M:System.String.IsNullOrEmpty(System.String)\"/>.</summary>");

        Assert.Equal("Stores incoming. See System.String.IsNullOrEmpty.", DocCommentText.RenderElement(summary));
    }

    [Fact]
    public void RenderExceptions_Cref_IncludesTypeName()
    {
        var member = XElement.Parse("<member><exception cref=\"T:System.ArgumentNullException\">When missing.</exception></member>");

        Assert.Equal("System.ArgumentNullException: When missing.", DocCommentText.RenderExceptions(member));
    }
}