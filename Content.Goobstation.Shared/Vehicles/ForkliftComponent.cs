using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Vehicles;

[RegisterComponent]
public sealed partial class ForkliftComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? LiftAction;


    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? UnliftAction;

    [DataField]
    public int ForkliftCapacity = 4;

    [DataField]
    public SoundSpecifier LiftSound;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? LiftSoundUid;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? LiftSoundEndTime;
}
