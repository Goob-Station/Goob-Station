using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonVoidCreatureComponent : Component
{
    /// <summary>
    /// The action entity spawned for this component.
    /// </summary>
    [ViewVariables]
    public EntityUid? ActionEnt;

    /// <summary>
    /// Prototype ID for the radial summon action.
    /// </summary>
    [ViewVariables]
    public EntProtoId ActionId = "ActionSummonVoidCreature";

    /// <summary>
    ///  The ghost entity to summon for the players to take over
    /// </summary>
    [DataField]
    public EntProtoId SummonId = "SummonVoidGhost";
}
