using System.Runtime.Loader;
using Doxie.Core.Services;

namespace Doxie.HelpFileGenerator;

internal class Program
{
    private static void Main(string[] args)
    {
        // TODO: Change this to the location where your assemblies are
        string assembliesPath = @"D:\Source\GitHub\Extenso\DoxieDummy\bin\Release\netcoreapp2.1";

        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            string path = Path.Combine(assembliesPath, name.Name + ".dll");

            return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
        };

        // TODO: Modify this to include the assemblies that you want to show in Doxie
        string[] selectedFiles = new[]
        {
            Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.dll"),
            Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.dll"),
            Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.Foundation.dll"),
            Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.JQueryUI.dll"),
            Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.KendoUI.dll"),
            Path.Combine(assembliesPath, "Extenso.AspNetCore.OData.dll"),
            Path.Combine(assembliesPath, "Extenso.Core.dll"),
            Path.Combine(assembliesPath, "Extenso.Data.dll"),
            Path.Combine(assembliesPath, "Extenso.Data.Entity.dll"),
            Path.Combine(assembliesPath, "Extenso.Data.MySql.dll"),
            Path.Combine(assembliesPath, "Extenso.Data.Npgsql.dll"),
            Path.Combine(assembliesPath, "Extenso.Data.QueryBuilder.dll"),
            Path.Combine(assembliesPath, "Extenso.Data.QueryBuilder.MySql.dll"),
            Path.Combine(assembliesPath, "Extenso.Data.QueryBuilder.Npgsql.dll")
        };

        string outputPath = assembliesPath;

        Console.WriteLine("Generating...");
        JsonHelpFileGenerator.Generate(selectedFiles, outputPath);
        Console.WriteLine("Done");

        Console.ReadLine();
    }
}