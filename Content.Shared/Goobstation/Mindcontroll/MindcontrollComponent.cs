using Content.Shared.Antag;
using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Mindcontroll;

[RegisterComponent, NetworkedComponent]
public sealed partial class MindcontrollComponent : Component
{
    [DataField]
    public EntityUid? Master = null;
    [DataField]
    public SoundSpecifier MindcontrollStartSound = new SoundPathSpecifier("/Audio/Goobstation/Ambience/Antag/mindcontroll_start.ogg");
    [DataField]
    public bool BriefingSent = false;
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> MindcontrollIcon { get; set; } = "MindcontrolledFaction";
}
