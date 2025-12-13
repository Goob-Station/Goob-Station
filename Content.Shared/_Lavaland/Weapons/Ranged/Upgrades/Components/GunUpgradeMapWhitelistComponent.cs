using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// Makes this gun upgrade work only on maps that pass some whitelists.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunUpgradeMapWhitelistComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;
}
