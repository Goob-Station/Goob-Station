using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Changeling.Components;

/// <summary>
///     Used for identifying other changelings.
///     Indicates that a changeling has bought the hivemind access ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HivemindComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "HivemindFaction";
}
