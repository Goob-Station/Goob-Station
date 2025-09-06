using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.FloorGoblin;

[RegisterComponent]
public sealed partial class StealShoesComponent : Component
{
    [DataField]
    public EntProtoId? ActionProto;

    [DataField]
    public EntityUid? StealAction;

    [DataField]
    public string ContainerId = "floorgoblin-shoes";

    [DataField]
    public SoundSpecifier? ChompSound = new SoundPathSpecifier("/Audio/Effects/bite.ogg");
}

public sealed partial class StealShoesEvent : EntityTargetActionEvent
{
}
