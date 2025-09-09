using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Heatlamp.Upgrades;

/// <summary>
///     Holds an upgrade to the stats on a heatlamp.
///     See HeatlampComponent for an explanation of4 the fields.
/// </summary>
[RegisterComponent]
public sealed partial class HeatlampStatsUpgradeComponent : Component
{
    [DataField]
    public float HeatingPowerDrain = 0f;

    [DataField]
    public float MaximumHeatingPerUpdate = 0f;

    [DataField]
    public float CoolingPowerDrain = 0f;

    [DataField]
    public float MaximumCoolingPerUpdate = 0f;

    [DataField]
    public DamageSpecifier ActivatedDamage = new ();
}
