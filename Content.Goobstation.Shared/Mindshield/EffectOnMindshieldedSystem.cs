using Content.Shared.Damage;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared.Goobstation.Mindshield;

public sealed class EffectOnMindshieldedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private readonly HashSet<Entity<MindShieldComponent>> _targets = new();
    private readonly Dictionary<(EntityUid Target, string Group), (EntityUid Source, TimeSpan Expiry)> _claims = new();
    private readonly List<(EntityUid Target, string Group)> _expired = new();
    private TimeSpan _nextPrune = TimeSpan.Zero;
    private static readonly TimeSpan PruneInterval = TimeSpan.FromSeconds(30);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EffectOnMindshieldedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EffectOnMindshieldedComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMapInit(Entity<EffectOnMindshieldedComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.Interval;
        Dirty(ent);
    }

    private void OnMeleeHit(Entity<EffectOnMindshieldedComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.MeleeDamage.Empty)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<MindShieldComponent>(hit))
                continue;

            _damageable.TryChangeDamage(hit, ent.Comp.MeleeDamage, ent.Comp.IgnoreResistances, origin: args.User);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var curTime = _timing.CurTime;

        if (curTime >= _nextPrune)
        {
            _nextPrune = curTime + PruneInterval;
            PruneClaims(curTime);
        }

        var query = EntityQueryEnumerator<EffectOnMindshieldedComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Range <= 0f || curTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = curTime + comp.Interval;
            Dirty(uid, comp);

            ApplyArea((uid, comp));
        }
    }

    private void ApplyArea(Entity<EffectOnMindshieldedComponent> ent)
    {
        var curTime = _timing.CurTime;

        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.Range, _targets);

        foreach (var target in _targets)
        {
            if (_mobState.IsDead(target))
                continue;

            if (!TryClaim(ent, target.Owner, curTime))
            {
                continue;
            }

            if (!ent.Comp.Damage.Empty)
            {
                _damageable.TryChangeDamage(target.Owner, ent.Comp.Damage, ent.Comp.IgnoreResistances, origin: ent.Owner);
            }

            foreach (var (effect, duration) in ent.Comp.StatusEffects)
            {
                _statusEffects.TryAddStatusEffectDuration(target.Owner, effect, duration);
            }
        }
    }

    private bool TryClaim(Entity<EffectOnMindshieldedComponent> ent, EntityUid target, TimeSpan curTime)
    {
        if (ent.Comp.Stacks)
        {
            return true;
        }

        var key = (target, ent.Comp.Group);

        if (_claims.TryGetValue(key, out var claim) && claim.Source != ent.Owner && curTime < claim.Expiry)
        {
            return false;
        }

        _claims[key] = (ent.Owner, curTime + ent.Comp.Interval * 2);
        return true;
    }

    private void PruneClaims(TimeSpan curTime)
    {
        _expired.Clear();

        foreach (var (key, claim) in _claims)
        {
            if (curTime >= claim.Expiry)
            {
                _expired.Add(key);
            }
        }

        foreach (var key in _expired)
        {
            _claims.Remove(key);
        }
    }
}
