using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MassWhisperComponent : Component
{
    /// <summary>
    /// Announce sound file path
    /// </summary>
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/Hastur/growl.ogg");

    /// <summary>
    /// Duration of madness
    /// </summary>
    [DataField]
    public float Duration = 10f;
}
