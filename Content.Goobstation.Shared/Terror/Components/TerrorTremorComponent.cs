using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// AoE stun.
/// </summary>
[RegisterComponent]
public sealed partial class TerrorTremorComponent : Component
{
    [DataField]
    public float Range = 3f;

    [DataField]
    public TimeSpan Stun = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan Knockdown = TimeSpan.FromSeconds(3);

    [DataField]
    public SoundCollectionSpecifier? Sound = new SoundCollectionSpecifier("XenoFootstepLarge");
}
