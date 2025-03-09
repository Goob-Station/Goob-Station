using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.ThrowableBlocker;

/// <summary>
/// Added to items that are able to block thrown objects. These items must be able to reflect projectiles for it to work.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ThrowableBlockerComponent : Component
{
    [DataField]
    public SoundSpecifier? BlockSound;
}
