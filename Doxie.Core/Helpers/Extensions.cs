using System.Collections.Generic;
using System.Linq;
using Doxie.Core.Models;

namespace Doxie.Core.Helpers
{
    public static class Extensions
    {
        public static ConstructorModel FindConstructor(this TypeModel typeBase, string id)
        {
            return typeBase.Constructors.FirstOrDefault(x => x.Id == id);
        }

        public static MethodModel FindMethod(this TypeModel typeBase, string id)
        {
            return typeBase.Methods.FirstOrDefault(x => x.Id == id);
        }

        public static PropertyModel FindProperty(this TypeModel typeBase, string id)
        {
            return typeBase.Properties.FirstOrDefault(x => x.Id == id);
        }

        public static TypeModel FindType(this IEnumerable<NamespaceModel> namespaces, string fullname)
        {
            return namespaces
                .Select(nameSpace => nameSpace.AllTypes.FirstOrDefault(x => x.FullName == fullname))
                .FirstOrDefault(x => x != null);
        }
    }
}