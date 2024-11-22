using Content.Shared.Antag;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.BloodCultist;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultLeaderComponent : Component, IAntagStatusIconComponent
{
    [DataField]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "BloodCultLeader";

    [DataField]
    public bool IconVisibleToGhost { get; set; } = true;
}
