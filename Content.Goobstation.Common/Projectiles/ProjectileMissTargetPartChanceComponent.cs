namespace Content.Goobstation.Common.Projectiles;

[RegisterComponent]
public sealed partial class ProjectileMissTargetPartChanceComponent : Component
{
    [DataField]
    public List<EntityUid> PerfectHitEntities = new();
}
