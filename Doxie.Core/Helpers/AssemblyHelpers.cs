using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Doxie.Core.Helpers
{
    public static class AssemblyHelpers
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException x)
            {
                return x.Types.Where(t => t != null);
            }
        }
    }
}