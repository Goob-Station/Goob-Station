using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Other;

public sealed class DamageNearbySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    private readonly HashSet<Entity<MobStateComponent>> _mobStates = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageNearbyComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<DamageNearbyComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextTick)
                continue;

            TryDamage((uid, comp));
            comp.NextTick = _timing.CurTime + comp.Delay;
            Dirty(uid, comp);
        }

    }

    private void OnMapInit(Entity<DamageNearbyComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTick = _timing.CurTime + ent.Comp.Delay;
        Dirty(ent);
    }

    private void TryDamage(Entity<DamageNearbyComponent> ent)
    {
        _mobStates.Clear();
        _lookup.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.Range, _mobStates);
        foreach (var entity in _mobStates)
        {
            if (!_whitelist.IsWhitelistPassOrNull(ent.Comp.Whitelist, entity))
                continue;

            _damageable.TryChangeDamage(entity, ent.Comp.Damage, targetPart: TargetBodyPart.All);
        }
    }
}
