using Robust.Shared.Audio;

namespace Content.Server._EinsteinEngines.Silicon;

/// <summary>
///     Applies a <see cref="SpamEmitSoundComponent"/> to a Silicon when its battery is drained, and removes it when it's not.
/// </summary>
[RegisterComponent]
public sealed partial class SiliconEmitSoundOnDrainedComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = default!;

    [DataField]
    public TimeSpan MinInterval = TimeSpan.FromSeconds(8);

    [DataField]
    public TimeSpan MaxInterval = TimeSpan.FromSeconds(15);

    [DataField]
    public float PlayChance = 1f;

    [DataField]
    public string? PopUp;
}
