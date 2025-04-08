using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeComponent : Component
{
    [DataField]
    public List<ProtoId<TagPrototype>> Tags = new();

    [DataField]
    public LocId ExamineText;

    [DataField]
    public int CapacityCost = 30; // By default drains 30% of the capacity.
}
