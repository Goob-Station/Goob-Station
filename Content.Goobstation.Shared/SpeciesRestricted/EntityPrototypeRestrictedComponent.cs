using Content.Shared.Inventory.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpeciesRestricted;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class EntityPrototypeRestrictedComponent : Component
{
    [DataField]
    public EntProtoId ProtoId;
}
