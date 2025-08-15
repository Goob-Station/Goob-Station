using Content.Server.Spreader;
using Content.Shared.Damage;

namespace Content.Server._Slon.Sm;

public sealed class CascadeSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<CascadeComponent, KudzuComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var cascade, out var kudzu, out var xform))
        {
            cascade.Timer -= frameTime;
            if (cascade.Timer > 0)
                continue;

            cascade.Timer = cascade.Interval;

            foreach (var ent in _lookup.GetEntitiesInRange(uid, cascade.Radius))
            {
                if (ent == uid)
                    continue;

                if (HasComp<CascadeComponent>(ent))
                    continue;
                _damageable.TryChangeDamage(ent, cascade.Damage, origin: uid);
            }
        }
    }
}
