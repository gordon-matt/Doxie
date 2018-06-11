using System;
using System.IO;
using System.Runtime.Loader;
using Doxie.Core.Services;

namespace Doxie.HelpFileGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string assembliesPath = @"D:\Source\GitHub\Extenso\DoxieDummy\bin\Release\netcoreapp2.1";

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                string path = Path.Combine(assembliesPath, name.Name + ".dll");

                if (File.Exists(path))
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                }

                return null;
            };

            var selectedFiles = new[]
            {
                Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.dll"),
                Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.dll"),
                Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.Foundation.dll"),
                Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.JQueryUI.dll"),
                Path.Combine(assembliesPath, "Extenso.AspNetCore.Mvc.ExtensoUI.KendoUI.dll"),
                Path.Combine(assembliesPath, "Extenso.AspNetCore.OData.dll"),
                Path.Combine(assembliesPath, "Extenso.Core.dll"),
                Path.Combine(assembliesPath, "Extenso.Data.dll"),
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
            
            // OLD
            //Console.WriteLine("Doxie JSON File Generator.");
            //Console.WriteLine("This tool will generate a JSON file for use in Doxie.");

            //Console.WriteLine("Enter path to the directory of assemblies you would like to use and ensure the XML documentation files are alongside them: ");
            //string assembliesPath = Console.ReadLine();

            //Console.WriteLine("Generating...");
            //var assemblyFinderService = new AssemblyFinderService(assembliesPath);
            //assemblyFinderService.GenerateJsonFile();

            //Console.WriteLine("Done! The assemblies.json file has been generated in the specified folder.");
            //Console.ReadLine();
        }
    }
}