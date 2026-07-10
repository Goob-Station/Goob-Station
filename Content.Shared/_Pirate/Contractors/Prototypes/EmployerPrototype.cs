using Content.Shared.Customization.Systems;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared._Pirate.Contractors.Prototypes;

/// <summary>
/// Prototype representing a character's employer in YAML.
/// </summary>
[Prototype("employer")]
public sealed partial class EmployerPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = string.Empty;

    [DataField]
    public string NameKey { get; private set; } = string.Empty;

    [DataField]
    public string DescriptionKey { get; private set; } = string.Empty;

    [DataField, ViewVariables]
    public HashSet<ProtoId<EmployerPrototype>> Rivals { get; private set; } = new();

    [DataField]
    public List<JobRequirement> Requirements = new();
}
