using Content.Shared._White.Standing;

namespace Content.Server._EinsteinEngines.TelescopicBaton;

[RegisterComponent]
public sealed partial class KnockdownOnHitComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField]
    public DropHeldItemsBehavior DropHeldItemsBehavior = DropHeldItemsBehavior.NoDrop;

    [DataField]
    public bool RefreshDuration = true;

    [DataField]
    public bool KnockdownOnHeavyAttack = true; // Goobstation
}
