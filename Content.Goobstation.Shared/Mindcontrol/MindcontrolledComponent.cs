using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mindcontrol;

[RegisterComponent, NetworkedComponent]
public sealed partial class MindcontrolledComponent : Component
{
    [DataField]
    public EntityUid? Master = null;
    [DataField]
    public SoundSpecifier MindcontrolStartSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/mindcontrol_start.ogg");
    [DataField]
    public bool BriefingSent = false;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> MindcontrolIcon { get; set; } = "MindcontrolledFaction";
}
