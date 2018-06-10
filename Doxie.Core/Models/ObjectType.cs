// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

namespace Doxie.Core.Models
{
    public enum ObjectType : byte
    {
        Unknown = 0,
        Structure = 1,
        Class = 2,
        Interface = 3,
        Enumeration = 4,
        Delegate = 5
    }
}