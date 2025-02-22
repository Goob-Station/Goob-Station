using Content.Shared.Wieldable;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Melee.Components;

/// <summary>
/// Indicates that this meleeweapon requires wielding to be useable.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(WieldableSystem))]
public sealed partial class MeleeRequiresWieldComponent : Component
{
    // Lavaland Change: The player will slip if they try to use the weapon without wielding it.
    [DataField]
    public bool FumbleOnAttempt = false;
}
