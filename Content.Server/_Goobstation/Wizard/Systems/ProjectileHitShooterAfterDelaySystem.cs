using Content.Server._Goobstation.Wizard.Components;
using Content.Shared.Projectiles;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class ProjectileHitShooterAfterDelaySystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ProjectileHitShooterAfterDelayComponent, ProjectileComponent>();
        while (query.MoveNext(out var uid, out var comp, out var projectile))
        {
            comp.Delay -= frameTime;

            if (comp.Delay > 0)
                continue;

            RemCompDeferred(uid, comp);
            projectile.IgnoreShooter = false;
            Dirty(uid, projectile);
        }
    }
}
