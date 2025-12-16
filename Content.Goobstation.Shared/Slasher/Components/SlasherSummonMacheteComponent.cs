using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher the ability to summon (or create) their machete into their active hand. Very generic.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherSummonMacheteComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherSummonMachete";

    /// <summary>
    /// Prototype id of the item to summon.
    /// </summary>
    [DataField]
    public EntProtoId MachetePrototype = "SlasherMachete";

    /// <summary>
    /// The current tracked machete entity.
    /// </summary>
    [ViewVariables]
    public EntityUid? MacheteUid;
}
