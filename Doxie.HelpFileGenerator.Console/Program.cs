using System;
using Doxie.Core.Services;

namespace Doxie.HelpFileGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Doxie JSON File Generator.");
            Console.WriteLine("This tool will generate a JSON file for use in Doxie.");

            Console.WriteLine("Enter path to the directory of assemblies you would like to use and ensure the XML documentation files are alongside them: ");
            string assembliesPath = Console.ReadLine();

            Console.WriteLine("Generating...");
            var assemblyFinderService = new AssemblyFinderService(assembliesPath);
            assemblyFinderService.GenerateJsonFile();

            Console.WriteLine("Done! The assemblies.json file has been generated in the specified folder.");
            Console.ReadLine();
        }
    }
}