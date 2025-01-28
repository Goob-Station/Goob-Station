using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BrigLockerComponent : Component
{
    [DataField]
    public bool Assigned = false;

    [DataField]
    public SoundSpecifier? DenySound;

    [DataField]
    public string Check = string.Empty;
}
