using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher the ability to instantly spawn a meat spike.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherSummonMeatSpikeComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherSummonMeatSpike";

    /// <summary>
    /// Prototype id of the spike to spawn.
    /// </summary>
    [DataField]
    public EntProtoId MeatSpikePrototype = "SlasherMeatSpike";
}

public sealed partial class SlasherSummonMeatSpikeEvent : InstantActionEvent
{
}
