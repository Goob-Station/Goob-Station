using Content.Goobstation.Shared.Retractable.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Retractable.Components;

/// <summary>
/// Component used as a marker for clothing summoned by the RetractableClothingAction system.
/// Used for keeping track of clothing summoned by said action.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(RetractableClothingSystem))]
public sealed partial class RetractableClothingComponent : Component
{
    /// <summary>
    /// The action that marked this item.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? SummoningAction;
}
