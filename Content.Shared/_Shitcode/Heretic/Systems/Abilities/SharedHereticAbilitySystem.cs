using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Body;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly EntityLookupSystem Lookup = default!;
    [Dependency] protected readonly StatusEffectsSystem Status = default!;
    [Dependency] private readonly StatusEffectNew.StatusEffectsSystem _statusNew = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedStarMarkSystem _starMark = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly PainSystem _pain = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    public static readonly DamageSpecifier AllDamage = new()
    {
        DamageDict =
        {
            {"Blunt", 1},
            {"Slash", 1},
            {"Piercing", 1},
            {"Heat", 1},
            {"Cold", 1},
            {"Shock", 1},
            {"Asphyxiation", 1},
            {"Bloodloss", 1},
            {"Caustic", 1},
            {"Poison", 1},
            {"Radiation", 1},
            {"Cellular", 1},
            {"Holy", 1},
        },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAsh();
        SubscribeBlade();
        SubscribeRust();
        SubscribeCosmos();
        SubscribeFlesh();
        SubscribeSide();

        SubscribeLocalEvent<HereticComponent, EventHereticShadowCloak>(OnShadowCloak);
    }

    protected List<Entity<MobStateComponent>> GetNearbyPeople(EntityUid ent,
        float range,
        string? path,
        EntityCoordinates? coords = null)
    {
        var list = new List<Entity<MobStateComponent>>();
        var lookup = Lookup.GetEntitiesInRange<MobStateComponent>(coords ?? Transform(ent).Coordinates, range);

        foreach (var look in lookup)
        {
            // ignore heretics with the same path*, affect everyone else
            if (TryComp<HereticComponent>(look, out var th) && th.CurrentPath == path || HasComp<GhoulComponent>(look))
                continue;

            if (!HasComp<StatusEffectsComponent>(look))
                continue;

            list.Add(look);
        }

        return list;
    }


    private void OnShadowCloak(Entity<HereticComponent> ent, ref EventHereticShadowCloak args)
    {
        if (!TryComp(ent, out StatusEffectsComponent? status))
            return;

        if (TryComp(ent, out ShadowCloakedComponent? shadowCloaked))
        {
            Status.TryRemoveStatusEffect(ent, args.Status, status, false);
            RemCompDeferred(ent.Owner, shadowCloaked);
            args.Handled = true;
            return;
        }

        // TryUseAbility only if we are not cloaked so that we can uncloak without focus
        // Ideally you should uncloak when losing focus but whatever
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;
        Status.TryAddStatusEffect<ShadowCloakedComponent>(ent, args.Status, args.Lifetime, true, status);
    }

    public bool TryUseAbility(EntityUid ent, BaseActionEvent args)
    {
        if (args.Handled)
            return false;

        // No using abilities while charging
        if (HasComp<RustChargeComponent>(ent))
            return false;

        if (!TryComp<HereticActionComponent>(args.Action, out var actionComp))
            return false;

        // check if any magic items are worn
        if (!TryComp<HereticComponent>(ent, out var hereticComp) || !actionComp.RequireMagicItem ||
            hereticComp.Ascended)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        var ev = new CheckMagicItemEvent();
        RaiseLocalEvent(ent, ev);

        if (ev.Handled)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        // Almost all of the abilites are serverside anyway
        if (_net.IsServer)
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-magicitem"), ent, ent);

        return false;
    }

    private EntityUid? GetTouchSpell<TEvent, TComp>(Entity<HereticComponent> ent, ref TEvent args)
        where TEvent : InstantActionEvent, ITouchSpellEvent
        where TComp : Component
    {
        if (!TryUseAbility(ent, args))
            return null;

        if (!TryComp(ent, out HandsComponent? hands) || hands.Hands.Count < 1)
            return null;

        args.Handled = true;

        var hasComp = false;

        foreach (var held in _hands.EnumerateHeld((ent, hands)))
        {
            if (!HasComp<TComp>(held))
                continue;

            hasComp = true;
            PredictedQueueDel(held);
        }

        if (hasComp || !_hands.TryGetEmptyHand((ent, hands), out var emptyHand))
            return null;

        var touch = PredictedSpawnAtPosition(args.TouchSpell, Transform(ent).Coordinates);

        if (_hands.TryPickup(ent, touch, emptyHand, animate: false, handsComp: hands))
            return touch;

        PredictedQueueDel(touch);
        return null;
    }

    protected EntityUid ShootProjectileSpell(EntityUid performer,
        EntityCoordinates coords,
        EntProtoId toSpawn,
        float speed,
        EntityUid? target)
    {
        var xform = Transform(performer);
        var fromCoords = xform.Coordinates;
        var toCoords = coords;

        var fromMap = _transform.ToMapCoordinates(fromCoords);
        var spawnCoords = _mapMan.TryFindGridAt(fromMap, out var gridUid, out _)
            ? _transform.WithEntityId(fromCoords, gridUid)
            : new(_map.GetMap(fromMap.MapId), fromMap.Position);

        var userVelocity = _physics.GetMapLinearVelocity(spawnCoords);

        var projectile = PredictedSpawnAtPosition(toSpawn, spawnCoords);
        var direction = _transform.ToMapCoordinates(toCoords).Position -
                        _transform.ToMapCoordinates(spawnCoords).Position;
        _gun.ShootProjectile(projectile, direction, userVelocity, performer, performer, speed);

        if (target != null)
            _gun.SetTarget(projectile, target.Value, out _);

        return projectile;
    }

    /// <summary>
    /// Heals everything imaginable
    /// </summary>
    /// <param name="uid">Entity to heal</param>
    /// <param name="toHeal">how much to heal, null = full heal</param>
    /// <param name="boneHeal">how much to heal bones, null = full heal</param>
    /// <param name="painHeal">how much to heal pain, null = full heal</param>
    public void IHateWoundMed(Entity<DamageableComponent?, BodyComponent?, ConsciousnessComponent?> uid,
        DamageSpecifier? toHeal,
        FixedPoint2? boneHeal,
        FixedPoint2? painHeal,
        FixedPoint2? woundHeal)
    {
        if (!Resolve(uid, ref uid.Comp1, false))
            return;

        if (toHeal != null)
        {
            _dmg.TryChangeDamage(uid,
                toHeal,
                true,
                false,
                uid.Comp1,
                targetPart: TargetBodyPart.All,
                splitDamage: SplitDamageBehavior.SplitEnsureAll);
        }
        else
        {
            TryComp<MobThresholdsComponent>(uid, out var thresholds);
            // do this so that the state changes when we set the damage
            _mobThreshold.SetAllowRevives(uid, true, thresholds);
            _dmg.SetAllDamage(uid, uid.Comp1, 0);
            _mobThreshold.SetAllowRevives(uid, false, thresholds);
        }

        if (Resolve(uid, ref uid.Comp2, false) && uid.Comp2.BodyType == BodyType.Complex && (boneHeal != FixedPoint2.Zero || woundHeal != FixedPoint2.Zero))
        {
            if (_body.TryGetRootPart(uid, out var rootPart, uid.Comp2))
            {
                foreach (var woundable in _wound.GetAllWoundableChildren(rootPart.Value))
                {
                    if (woundHeal != FixedPoint2.Zero)
                    {
                        _wound.TryHaltAllBleeding(woundable.Owner, woundable.Comp, true);
                        if (woundHeal == null)
                            _wound.ForceHealWoundsOnWoundable(woundable.Owner, out _, null, woundable.Comp);
                        else
                            _wound.TryHealWoundsOnWoundable(woundable.Owner, -woundHeal.Value, out _, woundable.Comp, null, true, true);
                    }

                    if (boneHeal == FixedPoint2.Zero)
                        continue;

                    if (woundable.Comp.Bone.ContainedEntities.FirstOrNull() is not { } bone ||
                        !TryComp(bone, out BoneComponent? boneComp))
                        continue;

                    if (boneHeal != null)
                        _trauma.ApplyDamageToBone(bone, boneHeal.Value, boneComp);
                    else
                        _trauma.SetBoneIntegrity(bone, boneComp.IntegrityCap, boneComp);
                }
            }
        }

        if (painHeal != FixedPoint2.Zero && Resolve(uid, ref uid.Comp3, false))
        {
            if (uid.Comp3.NerveSystem != default)
            {
                foreach (var painModifier in uid.Comp3.NerveSystem.Comp.Modifiers)
                {
                    if (painHeal != null && painModifier.Value.Change > -painHeal.Value)
                    {
                        // This reduces pain maybe, who the hell knows
                        _pain.TryChangePainModifier(uid.Comp3.NerveSystem.Owner,
                            painModifier.Key.Item1,
                            painModifier.Key.Item2,
                            painModifier.Value.Change + painHeal.Value,
                            uid.Comp3.NerveSystem.Comp);
                        continue;
                    }

                    _pain.TryRemovePainModifier(uid.Comp3.NerveSystem.Owner,
                        painModifier.Key.Item1,
                        painModifier.Key.Item2,
                        uid.Comp3.NerveSystem.Comp);
                }

                foreach (var painMultiplier in uid.Comp3.NerveSystem.Comp.Multipliers)
                {
                    // Uhh... just fucking remove it, who cares
                    _pain.TryRemovePainMultiplier(uid.Comp3.NerveSystem.Owner,
                        painMultiplier.Key,
                        uid.Comp3.NerveSystem.Comp);
                }

                foreach (var nerve in uid.Comp3.NerveSystem.Comp.Nerves)
                {
                    foreach (var painFeelsModifier in nerve.Value.PainFeelingModifiers)
                    {
                        // Idk what it does, just remove it
                        _pain.TryRemovePainFeelsModifier(painFeelsModifier.Key.Item1,
                            painFeelsModifier.Key.Item2,
                            nerve.Key,
                            nerve.Value);
                    }
                }
            }

            foreach (var multiplier in
                     uid.Comp3.Multipliers.Where(multiplier => multiplier.Value.Type == ConsciousnessModType.Pain))
            {
                // Wtf is consciousness???
                _consciousness.RemoveConsciousnessMultiplier(uid,
                    multiplier.Key.Item1,
                    multiplier.Key.Item2,
                    uid.Comp3);
            }

            foreach (var modifier in
                     uid.Comp3.Modifiers.Where(modifier => modifier.Value.Type == ConsciousnessModType.Pain))
            {
                // Read this method name
                _consciousness.RemoveConsciousnessModifier(uid, modifier.Key.Item1, modifier.Key.Item2, uid.Comp3);
            }
        }
    }

    public virtual void InvokeTouchSpell<T>(Entity<T> ent, EntityUid user) where T : Component, ITouchSpell
    {
        _audio.PlayPredicted(ent.Comp.Sound, user, user);
    }

    protected virtual void SpeakAbility(EntityUid ent, HereticActionComponent args) { }
}
