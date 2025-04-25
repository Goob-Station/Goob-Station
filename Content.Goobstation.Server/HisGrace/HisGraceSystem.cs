// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.HisGrace;
using Content.Goobstation.Shared.Overlays;
using Content.Server.Atmos.Components;
using Content.Server.Item;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.HisGrace;

public sealed partial class HisGraceSystem : SharedHisGraceSystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly PopupSystem _popup = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MobStateSystem _state = null!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = null!;
    [Dependency] private readonly TransformSystem _transform = null!;
    [Dependency] private readonly AudioSystem _audio = null!;
    [Dependency] private readonly MindSystem _mind = null!;
    [Dependency] private readonly StunSystem _stun = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HisGraceComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<HisGraceComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<HisGraceComponent, GotUnequippedHandEvent>(OnUnequipped);
        SubscribeLocalEvent<HisGraceComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<HisGraceComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<HisGraceComponent, HisGraceHungerChangedEvent>(OnHungerChanged);
        SubscribeLocalEvent<HisGraceComponent, HisGraceEntityConsumedEvent>(OnEntityConsumed);
        SubscribeLocalEvent<HisGraceUserComponent, RefreshMovementSpeedModifiersEvent>(OnModifierRefresh);
    }

    private void OnInit(EntityUid uid, HisGraceComponent component, MapInitEvent args)
    {
        component.Stomach = _containerSystem.EnsureContainer<Container>(uid, "stomach");

        if (!TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        component.BaseDamage = melee.Damage;
        component.OrderedStates = component.StateThresholds.OrderBy(t => t.Value.Threshold).ToList();
    }

    private void OnEquipped(EntityUid uid, HisGraceComponent component, ref GotEquippedHandEvent args)
    {
        component.IsHeld = true;
        component.Holder = args.User;
    }

    private void OnUnequipped(EntityUid uid, HisGraceComponent component, ref GotUnequippedHandEvent args)
    {
        component.IsHeld = false;
        component.Holder = null;
    }

    private void OnMeleeHit(EntityUid uid, HisGraceComponent comp, ref MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
            TryDevour(comp, hitEntity);
    }

    private void OnModifierRefresh(EntityUid uid, HisGraceUserComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(comp.SpeedMultiplier);
    }

    private void OnUse(EntityUid uid, HisGraceComponent comp, ref UseInHandEvent args)
    {
        if (comp.CurrentState != HisGraceState.Dormant)
            return;

        comp.User = args.User;
        EnsureComp<HisGraceUserComponent>(args.User);
        _speedModifier.RefreshMovementSpeedModifiers(args.User);

        var popUp = Loc.GetString("hisgrace-use-start");
        _popup.PopupEntity(popUp, args.User, args.User, PopupType.MediumCaution);

        ChangeState(comp, HisGraceState.Peckish);
    }

    private void OnEntityConsumed(EntityUid uid, HisGraceComponent comp, ref HisGraceEntityConsumedEvent args)
    {
        comp.EntitiesAbsorbed++;

        if (comp.EntitiesAbsorbed >= comp.AscensionThreshold)
            ChangeState(comp, HisGraceState.Ascended);

        if (!TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        // 5 blunt per entity consumed
        comp.CurrentDamageIncrease.DamageDict["Blunt"] = comp.EntitiesAbsorbed * 5;
        melee.Damage = comp.BaseDamage + comp.CurrentDamageIncrease;
    }

    private void OnHungerChanged(EntityUid uid, HisGraceComponent comp, ref HisGraceHungerChangedEvent args)
    {
        if (comp.User is not { } user)
            return;

        _speedModifier.RefreshMovementSpeedModifiers(user);

        if (!TryComp<HisGraceUserComponent>(comp.User, out var userComp))
            return;

        if (args.NewState == HisGraceState.Ascended && args.OldState != HisGraceState.Ascended && TryComp<AppearanceComponent>(uid, out var appearanceComponent))
        {
            EnsureComp<UnremoveableComponent>(uid);
            DoAscension(comp);
            DoAscensionVisuals((uid, comp), "ascended");

            return;
        }

        if (args.NewState > args.OldState && args.OldState is not HisGraceState.Dormant && args.NewState is not HisGraceState.Ascended)
        {
            var increasePopup = Loc.GetString("hisgrace-hunger-increased");
            _popup.PopupEntity(increasePopup, uid, PopupType.MediumCaution);
        }

        if (args.NewState < args.OldState)
        {
            var decreasePopup = Loc.GetString("hisgrace-hunger-decreased");
            _popup.PopupEntity(decreasePopup, uid, PopupType.Medium);
        }

        switch (args.NewState)
        {
            case HisGraceState.Dormant:
            {
                RemComp<UnremoveableComponent>(uid);
                RemComp<JitteringComponent>(uid);

                var dormantPopup = Loc.GetString("hisgrace-hunger-sated");
                _popup.PopupEntity(dormantPopup, uid, PopupType.MediumCaution);

                // reset user to null
                comp.User = null;

                // free all eaten entities
                var toRelease = new List<EntityUid>(comp.Stomach.ContainedEntities);
                foreach (var ent in toRelease)
                {
                    _containerSystem.TryRemoveFromContainer(ent);
                    _stun.TryParalyze(ent, TimeSpan.FromSeconds(8), true);
                }

                break;
            }
            case HisGraceState.Peckish:
            {
                RemComp<UnremoveableComponent>(uid);
                RemComp<JitteringComponent>(uid);
                userComp.SpeedMultiplier = userComp.BaseSpeedMultiplier;
                userComp.SpeedMultiplier += comp.SpeedAddition;

                break;
            }

            case HisGraceState.Ravenous:
            case HisGraceState.Starving:
            {
                EnsureComp<UnremoveableComponent>(uid);
                EnsureComp<JitteringComponent>(uid);
                userComp.SpeedMultiplier = userComp.BaseSpeedMultiplier;
                userComp.SpeedMultiplier += comp.SpeedAddition * 2;
                break;
            }

            case HisGraceState.Death:
            {
                _damageable.TryChangeDamage(user, comp.DamageOnFail, targetPart: TargetBodyPart.Head,  origin: uid, ignoreResistances: true);

                var popUp = Loc.GetString("hisgrace-death", ("target", Name(user)));
                _popup.PopupEntity(popUp, user, user, PopupType.LargeCaution);


                ChangeState(comp, HisGraceState.Dormant);
                break;
            }
        }
    }

    #region Update Loop

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HisGraceComponent, MeleeWeaponComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var hisGrace, out var melee, out var xform))
        {
            // Don't update anything if it's dormant
            if (hisGrace.CurrentState is HisGraceState.Dormant or HisGraceState.Death or HisGraceState.Ascended)
                continue;

            if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(hisGrace.User))
            {
                hisGrace.CurrentState = HisGraceState.Dormant;
                continue;
            }

            var nearbyEnts = _lookup.GetEntitiesInRange(uid, 1f);

            // Handle damaging user if too far away.
            if (hisGrace.User != null && !nearbyEnts.Contains(hisGrace.User.Value) && _timing.CurTime > hisGrace.NextUserAttack)
            {
                var popUp = Loc.GetString("hisgrace-too-far");
                _popup.PopupEntity(popUp, hisGrace.User.Value, hisGrace.User.Value, PopupType.LargeCaution);
                _damageable.TryChangeDamage(hisGrace.User.Value, hisGrace.BaseDamage, targetPart: TargetBodyPart.Head, ignoreResistances: true);

                hisGrace.NextUserAttack = _timing.CurTime + hisGrace.TickDelay;
            }

            // Handle attacking when not held
            if (!hisGrace.IsHeld || hisGrace.Holder != hisGrace.User)
            {
                foreach (var entity in nearbyEnts.Where(entity => HasComp<MobStateComponent>(entity) && entity != hisGrace.User
                             &&  _timing.CurTime > hisGrace.NextGroundAttack
                             && !TerminatingOrDeleted(entity)
                             && !_containerSystem.IsEntityOrParentInContainer(entity)))
                {
                    var coordinates = _transform.GetMapCoordinates(uid);
                    var angle = _transform.GetRelativePosition(xform, entity, GetEntityQuery<TransformComponent>()).ToAngle();

                    _damageable.TryChangeDamage(entity, melee.Damage, targetPart: TargetBodyPart.Head, origin: uid);
                    _audio.PlayPvs(melee.HitSound, uid);
                    _popup.PopupEntity(Loc.GetString("hisgrace-attack-popup", ("target", Name(entity))), uid, PopupType.LargeCaution);
                    _melee.DoLunge(uid, uid, angle, coordinates.Position, null, angle, false, false);

                    TryDevour(hisGrace, entity);
                    hisGrace.NextGroundAttack = _timing.CurTime + hisGrace.TickDelay;

                    break;
                }
            }

            // Update hunger based on threshold.
            var downgradeNeeded = hisGrace.StateThresholds.TryGetValue(hisGrace.CurrentState, out var currentThreshold)
                                  && hisGrace.Hunger < currentThreshold.Threshold;

            for (var i = hisGrace.OrderedStates.Count - 1; i >= 0; i--)
            {
                var threshold = hisGrace.OrderedStates[i];
                if (threshold.Key > hisGrace.CurrentState)
                {
                    if (hisGrace.Hunger >= threshold.Value.Threshold)
                    {
                        hisGrace.HungerIncrement = threshold.Value.Increment;
                        ChangeState(hisGrace, threshold.Key);
                        break;
                    }
                }
                else if (downgradeNeeded && hisGrace.Hunger >= threshold.Value.Threshold)
                {
                    hisGrace.HungerIncrement = threshold.Value.Increment;
                    ChangeState(hisGrace, threshold.Key);
                    break;
                }
            }

            if (hisGrace.NextHungerTick > _timing.CurTime)
                continue;

            if (!TerminatingOrDeleted(hisGrace.User))
                _damageable.TryChangeDamage(hisGrace.User, hisGrace.Healing);

            hisGrace.Hunger = Math.Clamp(hisGrace.Hunger + hisGrace.HungerIncrement, 0, 200);
            hisGrace.NextHungerTick = _timing.CurTime + hisGrace.TickDelay;
        }

    }

    #endregion

    #region Helpers

    private void DoAscension(HisGraceComponent comp)
    {
        if (comp.User is not { } user|| TerminatingOrDeleted(user))
            return;

        var ascensionPopup = Loc.GetString("hisgrace-ascension");
        _popup.PopupEntity(ascensionPopup, user, user, PopupType.Large);

        EnsureComp<ThermalVisionComponent>(user);
        EnsureComp<PressureImmunityComponent>(user);
        EnsureComp<BreathingImmunityComponent>(user);

        _damageable.SetDamageModifierSetId(user, comp.AscensionDamageSet);

    }

    private void ChangeState(HisGraceComponent comp, HisGraceState newState)
    {
        var oldstate = comp.CurrentState;
        comp.CurrentState = newState;

        var ev = new HisGraceHungerChangedEvent(newState, oldstate);
        RaiseLocalEvent(comp.Owner, ref ev);
    }

    private bool TryDevour(HisGraceComponent comp, EntityUid target)
    {
        if (!_state.IsIncapacitated(target) || !_containerSystem.Insert(target, comp.Stomach) )
            return false;

        // Hunger gained from eating an entity is 20% of their
        comp.Hunger -= GetHungerValue(target, comp).Value;

        var devourPopup = Loc.GetString("hisgrace-devour", ("target", Name(target)));
        _audio.PlayPvs(comp.SoundDevour, target);
        _popup.PopupEntity(devourPopup, target, PopupType.LargeCaution);

        // don't apply bonuses for enities consumed that don't have minds or aren't human (no farming sentient mice)
        if (_mind.TryGetMind(target, out _, out _) && HasComp<HumanoidAppearanceComponent>(target))
        {
            var ev = new HisGraceEntityConsumedEvent();
            RaiseLocalEvent(comp.Owner, ref ev);
        }

        return true;
    }

    private FixedPoint2 GetHungerValue(EntityUid target, HisGraceComponent comp)
    {
        if (!TryComp<MobThresholdsComponent>(target, out var mobThresholds))
            return comp.HungerOnDevourDefault;

        var thresholds = mobThresholds.Thresholds;
        var (criticalThreshold, value) = thresholds.FirstOrDefault(kvp => kvp.Value == MobState.Critical);

        if (value != MobState.Critical)
            return comp.HungerOnDevourDefault;

        // hunger value is equal to the mutiplier times the crit threshold.
        // this is twenty for humans
        return comp.HungerOnDevourMultiplier * criticalThreshold.Value;
    }

    #endregion

}
