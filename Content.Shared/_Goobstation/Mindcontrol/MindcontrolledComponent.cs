using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Mindcontrol;

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
