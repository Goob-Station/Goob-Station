using Content.Shared.Construction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction.Components;

/// <summary>
/// Goobstation
/// Knowledge component that contains information about all available crafting recipes.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConstructionKnowledgeComponent : Component
{
    /// <summary>
    /// All available groups of constructions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<ConstructionGroupPrototype>> Groups = new();
}
