using Content.Shared.Construction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction.Components;

/// <summary>
/// Grants knowledge about constructions to the entity on mapinit.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ConstructionKnowledgeGrantComponent : Component
{
    /// <summary>
    /// Groups of constructions that we are adding.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ConstructionGroupPrototype>> Groups = new();
}
