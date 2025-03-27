using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunComponentUpgradeComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
