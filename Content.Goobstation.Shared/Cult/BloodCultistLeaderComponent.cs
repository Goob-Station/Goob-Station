using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult;

/// <summary>
///    Marks the leader of the blood cult.
/// </summary>
[RegisterComponent, NetworkedComponent, Virtual]
public partial class BloodCultistLeaderComponent : Component
{
    [DataField] public ProtoId<FactionIconPrototype> StatusIcon = "BloodCultistLeader";
}
