using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.StationRadio.Components;

[RegisterComponent]
public sealed partial class CdComponent : Component
{
    /// <summary>
    /// What audio should be played whenever the CD is played
    /// </summary>
    [DataField] public SoundPathSpecifier? Audio;
}
