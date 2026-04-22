using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Components;

/// <summary>
/// Handles Devil authority action variables which fetches all slaughter demons and teleports them to the Devil.
/// Spawns a massive pool of blood under the devil regardless of it being succesfull or not. And also stuns anyone nearby.
/// </summary>

[RegisterComponent]
public sealed partial class DevilAuthorityComponent : Component
{
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(2);

    [DataField]
    public float StunRange = 8f;

    [DataField]
    public EntProtoId BloodPuddleProto = "PuddleBloodMassive";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Devil/archscream.ogg");

    [DataField]
    public LocId Invocation = "devil-speech-authority";
}
