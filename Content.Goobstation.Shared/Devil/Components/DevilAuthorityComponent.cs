using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Devil.Components;

[RegisterComponent]
public sealed partial class DevilAuthorityComponent : Component
{
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(2);

    [DataField]
    public float StunRange = 8f;

    [DataField]
    public string BloodPuddleProto = "PuddleBloodMassive";

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Devil/archscream.ogg");

    [DataField]
    public LocId Invocation = "devil-speech-authority";
}
