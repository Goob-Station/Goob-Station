using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Components;

/// <summary>
/// Lets an entity spawn in an object into their hand. No other behaviour like recalling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DevilSummonPitchforkComponent : Component
{
    /// <summary>
    /// Prototype ID of the item to summon.
    /// </summary>
    [DataField]
    public EntProtoId Prototype = "DevilPitchfork";
}
