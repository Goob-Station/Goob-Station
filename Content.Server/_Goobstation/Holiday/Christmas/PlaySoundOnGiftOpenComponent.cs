using Robust.Shared.Audio;

namespace Content.Server._Goobstation.Holiday.Christmas;

/// <summary>
/// Specifies a unique sound that is to be played when the component holder is opened from a gift. This is played alongside the default gift-opening noise. Refer to the RandomGiftSystem for further details.
/// </summary>
[RegisterComponent]
public sealed partial class PlaySoundOnGiftOpenComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound; // The sound that is played alongside the gift unboxing.

    [DataField]
    public float SoundVolume; // The volume of the sound that is played.

    [DataField]
    public bool IsSoundGlobal = false; // Determines if the sound is played globally (true) or locally (false). Set default to false as a safety measure.
}
