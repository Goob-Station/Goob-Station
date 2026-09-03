using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ParallaxStargaze;

[RegisterComponent, NetworkedComponent]
public sealed partial class ParallaxStargazeComponent : Component
{
    [DataField]
    public float ActivationRadius = 5f;

    [DataField]
    public float StillTime = 6f;

    [DataField]
    public float FadeTime = 2.5f;

    [DataField]
    public float MobRadius = 5f;

    [DataField]
    public float MoveThreshold = 0.1f;

    [DataField]
    public SoundSpecifier Music = new SoundCollectionSpecifier("AmbienceSpaceChill");
}
