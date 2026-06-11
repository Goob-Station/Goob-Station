using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Applied to entities who have had their soul stolen. Prevents the slasher from stealing from the same person multiple times.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SoullessComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> FactionIcon = "SoullessFaction";
}
