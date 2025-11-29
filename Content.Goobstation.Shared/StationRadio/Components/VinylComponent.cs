using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.StationRadio.Components;

[RegisterComponent]
public sealed partial class VinylComponent : Component
{
    /// <summary>
    /// What song should be played when the vinyl is played
    /// </summary>
    [DataField] public SoundPathSpecifier? Song;
}
