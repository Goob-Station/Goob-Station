using Content.Goobstation.Shared.BerserkerImplant;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Damage;

namespace Content.Goobstation.Server.BerserkerImplant;

public sealed class BerserkerImplantSystem : SharedBerserkerImplantSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BerserkerImplantActiveComponent, ComponentRemove>(OnShutdown);
    }

    private void OnShutdown(Entity<BerserkerImplantActiveComponent> ent, ref ComponentRemove args)
    {
        _damageable.TryChangeDamage(ent.Owner, ent.Comp.DelayedDamage, true);
        RemComp<TrailComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = Timing.CurTime;
        var query = EntityQueryEnumerator<BerserkerImplantActiveComponent>();

        while (query.MoveNext(out var ent, out var berserker))
        {
            if (berserker.EndTime > curTime)
                continue;

            RemCompDeferred<BerserkerImplantActiveComponent>(ent);
        }
    }
}
