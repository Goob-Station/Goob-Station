using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class UpgradeableGunComponent : Component
{
    [DataField]
    public string UpgradesContainerId = "upgrades";

    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public int? MaxUpgradeCount;

    [DataField]
    public int MaxUpgradeCapacity = 100;
}
