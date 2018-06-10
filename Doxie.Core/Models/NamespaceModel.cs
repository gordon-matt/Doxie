// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Doxie.Core.Models
{
    public class NamespaceModel
    {
        public NamespaceModel()
        {
            Classes = new List<TypeModel>();
            Interfaces = new List<TypeModel>();
            Structures = new List<TypeModel>();
            Enumerations = new List<TypeModel>();
            Delegates = new List<TypeModel>();
        }

        public ICollection<TypeModel> AllTypes => Classes
            .Union(Interfaces)
            .Union(Structures)
            .Union(Enumerations)
            .Union(Delegates)
            .OrderBy(t => t.Name)
            .ToList();

        public ICollection<TypeModel> Classes { get; set; }

        public int CountAlltype => AllTypes.Count;

        public ICollection<TypeModel> Delegates { get; set; }

        public ICollection<TypeModel> Enumerations { get; set; }

        public ICollection<TypeModel> Interfaces { get; set; }

        public string Name { get; set; }

        public ICollection<TypeModel> Structures { get; set; }

        public override bool Equals(object obj)
        {
            return obj is NamespaceModel b && Name == b.Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}