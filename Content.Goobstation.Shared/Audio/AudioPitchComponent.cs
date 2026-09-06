using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Audio;

/// <summary>
/// Changes the pitch of everything that has this.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AudioPitchComponent : Component
{
    [DataField]
    public float Pitch = 0.7f;
}
