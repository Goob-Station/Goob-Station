using Robust.Shared.Audio;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Possession;


[RegisterComponent]
public sealed partial class PossessedComponent : Component
{
    [DataField]
    public EntityUid OriginalMindId { get; set; }
    [DataField]
    public EntityUid PossessorMindId { get; set; }
    [DataField]
    public EntityUid PossessorOriginalEntity { get; set; }

    [DataField]
    public TimeSpan PossessionEndTime { get; set; }

    [DataField]
    public TimeSpan PossessionTimeRemaining;

    [DataField]
    public bool WasPacified;

    [DataField]
    public SoundPathSpecifier PossessionSoundPath = new ("/Audio/_Goobstation/Effects/bone_crack.ogg");
}
