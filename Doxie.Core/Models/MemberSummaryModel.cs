// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

namespace Doxie.Core.Models;

/// <summary>A short name and description used in a type's member index.</summary>
public class MemberSummaryModel
{
    /// <summary>Text shown beside the member name.</summary>
    public string Description { get; set; }

    /// <summary>Member name.</summary>
    public string Name { get; set; }
}