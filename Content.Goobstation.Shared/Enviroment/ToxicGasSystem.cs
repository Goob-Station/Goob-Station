using Content.Shared.Damage;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Enviroment;
public sealed class ToxicGasSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Update(float frameTime)
    {
        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<ToxicGasDamageComponent, TransformComponent>();

        while (query.MoveNext(out var gasEnt, out var gas, out var xform))
        {
            gas.Accumulator += frameTime;

            if (gas.Accumulator < gas.Interval)
                continue;

            gas.Accumulator -= gas.Interval;

            foreach (var entity in _lookup.GetEntitiesInRange(xform.Coordinates, 1.0f))
            {
                if (!HasComp<DamageableComponent>(entity))
                    continue;

                if (HasComp<ToxicGasImmunityComponent>(entity))
                    continue;

                _damageable.TryChangeDamage(entity, gas.Damage, true);
            }
        }
    }

}
