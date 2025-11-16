using Content.Shared.Construction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction.Components;

/// <summary>
/// Goobstation
/// Knowledge component that contains information about all available crafting recipes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ConstructionKnowledgeComponent : Component
{
    /// <summary>
    /// Group that this knowledge grants.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ConstructionGroupPrototype> Group;
}
