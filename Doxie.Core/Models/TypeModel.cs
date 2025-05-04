﻿// ----------------------------------------------------------------------------
// Based on AutoHelp's implementation
// Original Code: https://github.com/RaynaldM/autohelp
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Doxie.Core.Models;

public class TypeModel : BaseCommentsModel
{
    public TypeModel()
    {
        Constructors = [];
        Methods = [];
        Properties = [];
        Members = [];
        Parents = [];
    }

    [JsonProperty]
    public ICollection<ConstructorModel> Constructors { get; set; }

    [JsonProperty]
    public bool IsAbstract { get; set; }

    [JsonProperty]
    public bool IsNested { get; set; }

    [JsonProperty]
    public bool IsPrimitive { get; set; }

    [JsonProperty]
    public bool IsPublic { get; set; }

    [JsonProperty]
    public bool IsSealed { get; set; }

    [JsonProperty]
    public ICollection<MemberSummaryModel> Members { get; set; }

    [JsonProperty]
    public ICollection<MethodModel> Methods { get; set; }

    [JsonIgnore]
    public NamespaceModel Namespace { get; set; }

    [JsonProperty]
    public ObjectType ObjectType { get; set; }

    [JsonProperty]
    public string ParentClass { get; set; }

    [JsonProperty]
    public ICollection<TypeSummaryModel> Parents { get; set; }

    [JsonProperty]
    public ICollection<PropertyModel> Properties { get; set; }
}