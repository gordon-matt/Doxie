using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Doxie.Core.Helpers;
using Doxie.Core.Models;
using Newtonsoft.Json;

namespace Doxie.Core.Services
{
    public interface IAssemblyFinderService
    {
        IEnumerable<AssemblyModel> GetAssemblies();

        AssemblyModel GetAssembly(Guid id);

        void GenerateJsonFile();
    }

    public class AssemblyFinderService : IAssemblyFinderService
    {
        private IEnumerable<AssemblyModel> assemblies;
        private string assembliesDirectoryPath;
        private string assembliesJsonFilePath;

        public AssemblyFinderService(string assembliesPath)
        {
            assembliesDirectoryPath = assembliesPath;
            assembliesJsonFilePath = Path.Combine(assembliesDirectoryPath, "assemblies.json");
            assemblies = InitAssemblies();
        }

        public IEnumerable<AssemblyModel> GetAssemblies()
        {
            return assemblies.Select(assembly => new AssemblyModel
            {
                Id = assembly.Id,
                Name = assembly.Name,
                FullName = assembly.FullName,
                Namespaces = null
            }).ToArray();
        }

        public AssemblyModel GetAssembly(Guid id)
        {
            return assemblies.SingleOrDefault(p => p.Id == id);
        }

        private IEnumerable<AssemblyModel> InitAssemblies()
        {
            return GetAssemblyFiles().Select(filePath => GetAssembly(filePath)).ToArray();
        }

        private IEnumerable<string> GetAssemblyFiles()
        {
            var assemblyFilePaths = new List<string>();
            if (!string.IsNullOrEmpty(assembliesDirectoryPath) && Directory.Exists(assembliesDirectoryPath))
            {
                var dllFilePaths = Directory.EnumerateFiles(assembliesDirectoryPath, "*.dll");
                assemblyFilePaths.AddRange(dllFilePaths.Where(file => File.Exists(file.Replace(".dll", ".xml"))));
            }

            return assemblyFilePaths;
        }

        private AssemblyModel GetAssembly(string filePath, bool parseNamespace = true)
        {
            var assembly = new DocParser().Parse(filePath, parseNamespace);
            assembly.FileName = filePath;
            return assembly;
        }

        public void GenerateJsonFile()
        {
            string json = JsonConvert.SerializeObject(assemblies);

            using (var fileStream = new FileStream(assembliesJsonFilePath, FileMode.Create, FileAccess.Write))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write(json);
            }
        }
    }
}