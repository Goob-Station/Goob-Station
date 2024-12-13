using Content.Shared.Wieldable;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.Ranged;

/// <summary>
/// Indicates that this gun user does not need to wield.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(WieldableSystem))]
public sealed partial class NoWieldNeededComponent : Component
{
    //If true, not only does the user not need to wield to fire, they get the bonus for free!
    [DataField("getBonus")]
    public bool GetBonus = true;
}
