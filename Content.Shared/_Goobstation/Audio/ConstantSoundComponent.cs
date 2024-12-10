using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Audio;

/// <summary>
///     Component that constantly plays a sound. That's it. I don't know why this didn't already exist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ConstantSoundComponent : Component
{
    [DataField("sound"), ViewVariables(VVAccess.ReadWrite)]
    public string? Sound = "/Audio/misc/notice1.ogg";

    [DataField("repeats"), ViewVariables(VVAccess.ReadWrite)]
    public bool Repeats = true;
}
