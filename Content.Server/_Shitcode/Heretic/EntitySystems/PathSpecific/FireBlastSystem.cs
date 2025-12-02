using System.Linq;
using Content.Goobstation.Common.Physics;
using Content.Goobstation.Common.Religion;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.StatusEffect;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Utility;

namespace Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;

public sealed class FireBlastSystem : SharedFireBlastSystem
{
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireBlastedComponent, ComponentRemove>(OnRemove);
    }

    private void OnRemove(Entity<FireBlastedComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        ClearBeamJoints((ent.Owner, ent.Comp));

        if (!ent.Comp.ShouldBounce || TrySendBeam(ent) || ent.Comp.HitEntities.Count < ent.Comp.BouncesForBonusEffect)
            return;

        BonusEffect(ent);
    }

    private void BonusEffect(Entity<FireBlastedComponent> origin)
    {
        var pos = Transform(origin).Coordinates;

        Spawn(origin.Comp.BonusEffect, pos);
        _audio.PlayPvs(origin.Comp.Sound, pos);

        var ghoulQuery = GetEntityQuery<GhoulComponent>();
        var hereticQuery = GetEntityQuery<HereticComponent>();
        var flammableQuery = GetEntityQuery<FlammableComponent>();
        var mobStateQuery = GetEntityQuery<MobStateComponent>();
        var dmgQuery = GetEntityQuery<DamageableComponent>();
        var statusQuery = GetEntityQuery<StatusEffectsComponent>();

        // Prioritize alive targets on fire, closest to origin
        var result = _lookup.GetEntitiesInRange(origin, origin.Comp.BonusRange, flags: LookupFlags.Dynamic)
            .Select(x => (x, flammableQuery.CompOrNull(x)))
            .Where(x => x.Item2 != null && x.Item1 != origin.Owner &&
                        (!hereticQuery.TryComp(x.Item1, out var heretic) || heretic.CurrentPath != "Ash") &&
                        !ghoulQuery.HasComp(x.Item1) &&
                        mobStateQuery.HasComp(x.Item1));

        foreach (var (uid, flam) in result)
        {
            _flammable.AdjustFireStacks(uid, origin.Comp.BonusFireStacks, flam, true, origin.Comp.FireProtectionPenetration);

            if (statusQuery.TryComp(uid, out var status))
                _stun.KnockdownOrStun(uid, origin.Comp.BonusKnockdownTime, true, status);

            if (!dmgQuery.TryComp(uid, out var dmg))
                continue;

            Dmg.TryChangeDamage(uid,
                origin.Comp.FireBlastBonusDamage,
                false,
                false,
                dmg,
                targetPart: TargetBodyPart.All,
                splitDamage: SplitDamageBehavior.SplitEnsureAll,
                canMiss: false);
        }
    }

    private bool TrySendBeam(Entity<FireBlastedComponent> origin)
    {
        // If the beam had already bounced at least once
        if (origin.Comp.HitEntities.Count > 0)
        {
            if (!TryComp(origin, out FlammableComponent? flammable))
                return false;

            if (!flammable.OnFire)
                return false;

            // Max bounces reached
            if (origin.Comp.HitEntities.Count >= origin.Comp.MaxBounces)
                return false;
        }

        var ghoulQuery = GetEntityQuery<GhoulComponent>();
        var hereticQuery = GetEntityQuery<HereticComponent>();
        var flammableQuery = GetEntityQuery<FlammableComponent>();
        var mobStateQuery = GetEntityQuery<MobStateComponent>();

        var xform = Transform(origin);
        var pos = Xform.GetWorldPosition(xform);

        // Prioritize alive targets on fire, closest to origin
        var result = _lookup.GetEntitiesInRange(origin, origin.Comp.FireBlastRange, flags: LookupFlags.Dynamic)
            .Select(x => (x, flammableQuery.CompOrNull(x), mobStateQuery.CompOrNull(x),
                (Xform.GetWorldPosition(x) - pos).LengthSquared()))
            .Where(x => x is { Item2: not null, Item3: not null } && x.Item1 != origin.Owner &&
                        (!hereticQuery.TryComp(x.Item1, out var heretic) || heretic.CurrentPath != "Ash") &&
                        !ghoulQuery.HasComp(x.Item1) &&
                        !Status.HasEffectComp<FireBlastedStatusEffectComponent>(x.Item1) &&
                        !origin.Comp.HitEntities.Contains(x.Item1))
            .OrderBy(x => x.Item3!.CurrentState)
            .ThenByDescending(x => x.Item2!.OnFire)
            .ThenBy(x => x.Item4)
            .FirstOrNull();

        if (result == null)
            return false;

        var (target, flam, _, _) = result.Value;

        var ev = new BeforeCastTouchSpellEvent(target);
        RaiseLocalEvent(target, ev, true);

        var antimagic = ev.Cancelled;

        var time = origin.Comp.BeamTime;

        if (antimagic)
            time *= 2;

        if (!Status.TrySetStatusEffectDuration(target, FireBlastStatusEffect, time))
            return false;

        var fireBlasted = EnsureComp<FireBlastedComponent>(target);
        fireBlasted.HitEntities = new(origin.Comp.HitEntities);
        fireBlasted.HitEntities.Add(origin);
        fireBlasted.Damage = antimagic ? 0f : 2f;
        fireBlasted.MaxBounces = origin.Comp.MaxBounces;
        fireBlasted.BeamTime = origin.Comp.BeamTime;
        Dirty(target, fireBlasted);

        // Send beam from target to origin so that we can easier remove it if we only have access to target
        var beam = EnsureComp<ComplexJointVisualsComponent>(target);
        beam.Data[GetNetEntity(origin)] =
            new ComplexJointVisualsData(origin.Comp.FireBlastBeamDataId, origin.Comp.FireBlastBeamSprite);
        Dirty(target, beam);

        _audio.PlayPvs(origin.Comp.Sound, xform.Coordinates);

        if (antimagic)
            return true;

        _flammable.AdjustFireStacks(target, origin.Comp.FireStacks, flam, true, origin.Comp.FireProtectionPenetration);

        Dmg.TryChangeDamage(target,
            origin.Comp.FireBlastDamage,
            origin: origin,
            targetPart: TargetBodyPart.All,
            splitDamage: SplitDamageBehavior.SplitEnsureAll,
            canMiss: false);

        return true;
    }

    protected override void BeamCollision(Entity<FireBlastedComponent> origin, EntityUid target)
    {
        base.BeamCollision(origin, target);

        var originPos = Xform.GetMapCoordinates(origin);
        var targetPos = Xform.GetMapCoordinates(target);

        var dir = (originPos.Position - targetPos.Position).Normalized();

        var ray = new CollisionRay(originPos.Position, dir, (int) CollisionGroup.Opaque);
        var result = _physics.IntersectRay(originPos.MapId, ray, origin.Comp.FireBlastRange, origin, false);

        var flammableQuery = GetEntityQuery<FlammableComponent>();
        var dmgQuery = GetEntityQuery<DamageableComponent>();
        var ghoulQuery = GetEntityQuery<GhoulComponent>();
        var hereticQuery = GetEntityQuery<HereticComponent>();
        var mobStateQuery = GetEntityQuery<MobStateComponent>();

        foreach (var ent in result)
        {
            if (ent.HitEntity == target)
                continue;

            if (!mobStateQuery.HasComp(ent.HitEntity))
                return;

            if (ghoulQuery.HasComp(ent.HitEntity))
                continue;

            if (hereticQuery.TryComp(ent.HitEntity, out var heretic) && heretic.CurrentPath == "Ash")
                continue;

            if (flammableQuery.TryComp(ent.HitEntity, out var flam))
                _flammable.AdjustFireStacks(ent.HitEntity, origin.Comp.CollisionFireStacks, flam, true, origin.Comp.FireProtectionPenetration);

            if (!dmgQuery.TryComp(ent.HitEntity, out var dmg))
                continue;

            Dmg.TryChangeDamage(ent.HitEntity,
                origin.Comp.FireBlastBeamCollideDamage,
                false,
                false,
                dmg,
                targetPart: TargetBodyPart.All,
                splitDamage: SplitDamageBehavior.SplitEnsureAll,
                canMiss: false);
        }
    }

    private void ClearBeamJoints(Entity<FireBlastedComponent, ComplexJointVisualsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp2, false))
            return;

        ent.Comp2.Data = ent.Comp2.Data.Where(x => x.Value.Id != ent.Comp1.FireBlastBeamDataId).ToDictionary();

        if (ent.Comp2.Data.Count == 0)
            RemComp(ent.Owner, ent.Comp2);
        else
            Dirty(ent.Owner, ent.Comp2);
    }
}
