using Doxie.Core.Solutions;

namespace Doxie.Core.Tests;

public class SolutionFileTests
{
    [Fact]
    public void GetProjects_Slnx_ReturnsProjectPaths()
    {
        string solution = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Doxie.slnx"));

        IReadOnlyList<string> projects = SolutionFile.GetProjects(solution);

        Assert.Contains(projects, path => path.EndsWith("Doxie.Core.csproj", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(projects, path => path.EndsWith("Doxie.HelpFileGenerator.GUI.csproj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(projects, path => path.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetProjects_Sln_SkipsSolutionFolders()
    {
        string directory = Path.Combine(Path.GetTempPath(), "doxie-sln-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string solution = Path.Combine(directory, "Sample.sln");
        File.WriteAllText(solution, """
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{11111111-1111-1111-1111-111111111111}"
            EndProject
            Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Sample.Core", "src\Sample.Core\Sample.Core.csproj", "{22222222-2222-2222-2222-222222222222}"
            EndProject
            """);

        IReadOnlyList<string> projects = SolutionFile.GetProjects(solution);

        Assert.Single(projects);
        Assert.EndsWith(Path.Combine("src", "Sample.Core", "Sample.Core.csproj"), projects[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Inspect_LibraryAndTestProject_SelectsOnlyTheLibrary()
    {
        string directory = Path.Combine(Path.GetTempPath(), "doxie-proj-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string library = Path.Combine(directory, "Sample.Core.csproj");
        string tests = Path.Combine(directory, "Sample.Core.Tests.csproj");
        File.WriteAllText(library, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        File.WriteAllText(tests, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="xunit" Version="2.9.3" />
              </ItemGroup>
            </Project>
            """);

        DocumentationTarget libraryTarget = ProjectInspector.Inspect(library);
        DocumentationTarget testTarget = ProjectInspector.Inspect(tests);

        Assert.True(libraryTarget.SuggestInclude);
        Assert.Equal("Library", libraryTarget.OutputType);
        Assert.Equal("Not built", libraryTarget.Status);
        Assert.False(testTarget.SuggestInclude);
        Assert.True(testTarget.IsTestProject);
    }
}
