using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeSystem))]
public sealed partial class GunComponentUpgrateComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
