// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Doxie.Core.Models
{
    public class AssemblyModel
    {
        public string FileName { get; set; }

        public string FullName { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public ICollection<NamespaceModel> Namespaces { get; set; }
    }
}