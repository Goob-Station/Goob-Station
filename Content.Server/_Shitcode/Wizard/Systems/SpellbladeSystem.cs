using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Lightning;
using Content.Shared._Goobstation.Wizard.Spellblade;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Electrocution;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class SpellbladeSystem : SharedSpellbladeSystem
{
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightningSpellbladeEnchantmentComponent, MeleeHitEvent>(OnLightningHit);
        SubscribeLocalEvent<FireSpellbladeEnchantmentComponent, MeleeHitEvent>(OnFireHit);
        SubscribeLocalEvent<SpacetimeSpellbladeEnchantmentComponent, MeleeHitEvent>(OnSpacetimeHit);
        SubscribeLocalEvent<ForceshieldSpellbladeEnchantmentComponent, MeleeHitEvent>(OnForceshieldHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        List<(EntityUid, TemporalSlashComponent, DamageableComponent, TransformComponent)> toDamage = new();

        var query = EntityQueryEnumerator<TemporalSlashComponent, DamageableComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var temporal, out var damageable, out var xform))
        {
            temporal.Accumulator += frameTime;

            if (temporal.Accumulator < temporal.HitDelay)
                continue;

            toDamage.Add((uid, temporal, damageable, xform));
        }

        foreach (var (uid, temporal, damageable, xform) in toDamage)
        {
            temporal.HitsLeft--;
            temporal.Accumulator = 0f;

            _damageable.TryChangeDamage(uid, temporal.Damage, damageable: damageable, targetPart: TargetBodyPart.Torso);
            Audio.PlayPvs(temporal.HitSound, xform.Coordinates);
            Spawn(temporal.Effect, xform.Coordinates);

            if (temporal.HitsLeft <= 0)
                RemCompDeferred(uid, temporal);
        }

        var shieldedQuery = EntityQueryEnumerator<ShieldedComponent>();
        while (shieldedQuery.MoveNext(out var uid, out var comp))
        {
            comp.Lifetime -= frameTime;
            if (comp.Lifetime <= 0f)
                RemCompDeferred(uid, comp);
        }
    }

    private void OnForceshieldHit(Entity<ForceshieldSpellbladeEnchantmentComponent> ent, ref MeleeHitEvent args)
    {
        var user = args.User;

        if (!args.IsHit || !args.HitEntities.Any(x => x != user && HasComp<MobStateComponent>(x)))
            return;

        var comp = ent.Comp;

        EnsureComp<ShieldedComponent>(user).Lifetime = comp.ShieldLifetime;
    }

    private void OnSpacetimeHit(Entity<SpacetimeSpellbladeEnchantmentComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        foreach (var entity in args.HitEntities)
        {
            if (!HasComp<DamageableComponent>(entity))
                continue;

            if (TryComp<TemporalSlashComponent>(entity, out var tempSlash))
            {
                tempSlash.HitsLeft += 2;
                tempSlash.HitDelay /= 2f;
                continue;
            }

            AddComp<TemporalSlashComponent>(entity).Damage = args.BaseDamage;
        }
    }

    protected override void AddIgniteOnMeleeHitComponent(EntityUid uid, float fireStacks)
    {
        base.AddIgniteOnMeleeHitComponent(uid, fireStacks);

        EnsureComp<IgniteOnMeleeHitComponent>(uid).FireStacks = fireStacks;
    }

    private void OnFireHit(Entity<FireSpellbladeEnchantmentComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        var (uid, comp) = ent;

        if (!TryComp(uid, out UseDelayComponent? useDelay))
            return;

        if (UseDelay.IsDelayed((uid, useDelay)))
            return;

        var coords = Transform(args.User).Coordinates;

        var success = false;
        var entities = _lookup.GetEntitiesInRange<FlammableComponent>(coords, comp.Range, LookupFlags.Dynamic);
        foreach (var (entity, flammable) in entities)
        {
            if (entity == args.User)
                continue;

            _flammable.AdjustFireStacks(entity, comp.FireStacks, flammable, true);
            success = true;
        }

        if (!success)
            return;

        UseDelay.TryResetDelay((uid, useDelay));
        Spawn(comp.Effect, coords);
        Audio.PlayPvs(comp.Sound, coords);
    }

    private void OnLightningHit(Entity<LightningSpellbladeEnchantmentComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        var (uid, comp) = ent;

        if (!TryComp(uid, out UseDelayComponent? useDelay))
            return;

        if (UseDelay.IsDelayed((uid, useDelay)))
            return;

        UseDelay.TryResetDelay((uid, useDelay));

        var performer = args.User;

        var action = new Action<EntityUid>(lightning =>
        {
            var preventCollide = EnsureComp<PreventCollideComponent>(lightning);
            preventCollide.Uid = performer;

            var electrified = EnsureComp<ElectrifiedComponent>(lightning);
            electrified.IgnoredEntity = performer;
            electrified.IgnoreInsulation = true;
            electrified.ShockDamage = comp.ShockDamage;
            electrified.SiemensCoefficient = comp.Siemens;
            electrified.ShockTime = comp.ShockTime;

            Entity<PreventCollideComponent, ElectrifiedComponent> entity = (lightning, preventCollide, electrified);
            Dirty(entity);
        });

        _lightning.ShootRandomLightnings(args.User,
            comp.Range,
            comp.BoltCount,
            comp.LightningPrototype,
            comp.ArcDepth,
            false,
            args.User,
            action);
    }
}
