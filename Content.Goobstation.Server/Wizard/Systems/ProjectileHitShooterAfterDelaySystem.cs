using Content.Goobstation.Server.Wizard.Components;
using Content.Shared.Projectiles;

namespace Content.Goobstation.Server.Wizard.Systems;

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
