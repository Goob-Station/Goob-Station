// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.HisGrace;
using Content.Goobstation.Shared.Overlays;
using Content.Server.Atmos.Components;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
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
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
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
    [Dependency] private readonly ChatSystem _chat = null!;
    [Dependency] private readonly MobThresholdSystem _threshold = null!;

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
        SubscribeLocalEvent<HisGraceUserComponent, AttackedEvent>(OnAttacked);
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

        if (!TryComp<StaminaComponent>(args.User, out var stamina))
            return;

        component.BaseStamCritThreshold = stamina.CritThreshold;
        stamina.CritThreshold = component.HoldingStamCritThreshold;

    }

    private void OnUnequipped(EntityUid uid, HisGraceComponent component, ref GotUnequippedHandEvent args)
    {
        component.IsHeld = false;
        component.Holder = null;

        if (TryComp<StaminaComponent>(args.User, out var stamina))
            stamina.CritThreshold = component.BaseStamCritThreshold;
    }

    private void OnMeleeHit(EntityUid uid, HisGraceComponent comp, ref MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
            TryDevour(comp, hitEntity);
    }

    private void OnAttacked(Entity<HisGraceUserComponent> ent, ref AttackedEvent args)
    {
        if (!TryComp<HisGraceComponent>(ent.Comp.HisGrace, out var hisGrace))
            return;

        // awaiting aviu code
        /*
        if (hisGrace.CurrentState == HisGraceState.Ascended)
            args.Damage *= ent.Comp.AscensionDamageCoefficient
        else
            args.Damage *= ent.Comp.DefaultDamageCoefficient
        */
    }

    private void OnModifierRefresh(EntityUid uid, HisGraceUserComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(comp.SpeedMultiplier);
    }

    private void UpdateSpeedMultiplier(HisGraceUserComponent userComp, float bonus)
    {
        userComp.SpeedMultiplier = userComp.BaseSpeedMultiplier + bonus;
    }

    private void OnUse(EntityUid uid, HisGraceComponent comp, ref UseInHandEvent args)
    {
        if (comp.CurrentState != HisGraceState.Dormant)
            return;

        comp.User = args.User;
        EnsureComp<HisGraceUserComponent>(args.User).HisGrace = uid;
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
        if (comp.User is not { } user
        || !TryComp<HisGraceUserComponent>(user, out var userComp))
            return;

        _speedModifier.RefreshMovementSpeedModifiers(user);

        if (HandleAscendedState(uid, comp, args, user))
            return;

        ShowHungerChangePopup(uid, args);
        HandleHungerState(uid, comp, user, userComp, args.NewState);
    }

    private bool HandleAscendedState(EntityUid uid, HisGraceComponent comp, HisGraceHungerChangedEvent args, EntityUid user)
    {
        if (args.NewState != HisGraceState.Ascended
        || args.OldState == HisGraceState.Ascended)
            return false;

        EnsureComp<UnremoveableComponent>(uid);
        DoAscension(comp);
        DoAscensionVisuals((uid, comp), "ascended");
        return true;
    }

    private void ShowHungerChangePopup(EntityUid uid, HisGraceHungerChangedEvent args)
    {
        // Prevents pop-up clutter.
        if (args.OldState == HisGraceState.Dormant)
            return;

        var (messageKey, popupType) = args.NewState > args.OldState &&
         args.NewState != HisGraceState.Ascended
         ? ("hisgrace-hunger-increased", PopupType.MediumCaution)
         : ("hisgrace-hunger-decreased", PopupType.Medium);

        _popup.PopupEntity(Loc.GetString(messageKey), uid, popupType);
    }

    private void HandleHungerState(EntityUid uid, HisGraceComponent comp, EntityUid user, HisGraceUserComponent userComp, HisGraceState newState)
    {
        switch (newState)
        {
            case HisGraceState.Dormant:
                HandleDormantState(uid, comp, user);
                break;
            case HisGraceState.Peckish:
                HandlePeckishState(uid, comp, userComp);
                break;
            case HisGraceState.Ravenous:
            case HisGraceState.Starving:
                HandleRavenousState(uid, comp, userComp);
                break;
            case HisGraceState.Death:
                HandleDeathState(uid, comp, user);
                break;
        }
    }
    private void HandleDormantState(EntityUid uid, HisGraceComponent comp, EntityUid user)
    {
        SetUnremovable(uid, false);
        _popup.PopupEntity(Loc.GetString("hisgrace-hunger-sated"), uid, PopupType.MediumCaution);
        comp.User = null;
        ReleaseContainedEntities(comp);
    }

    private void HandlePeckishState(EntityUid uid, HisGraceComponent comp, HisGraceUserComponent userComp)
    {
        SetUnremovable(uid, false);
        UpdateSpeedMultiplier(userComp, comp.SpeedAddition);
    }

    private void HandleRavenousState(EntityUid uid, HisGraceComponent comp, HisGraceUserComponent userComp)
    {
        SetUnremovable(uid, true);
        UpdateSpeedMultiplier(userComp, comp.SpeedAddition * 2);
    }

    private void HandleDeathState(EntityUid uid, HisGraceComponent comp, EntityUid user)
    {
        SetUnremovable(uid, true);
        _damageable.TryChangeDamage(user,
            comp.DamageOnFail,
            targetPart: TargetBodyPart.Head,
            origin: uid,
            ignoreResistances: true);

        var popup = Loc.GetString("hisgrace-death", ("target", Name(user)));
        _popup.PopupEntity(popup, user, user, PopupType.LargeCaution);

        ChangeState(comp, HisGraceState.Dormant);
    }


    #region Update Loop

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HisGraceComponent, MeleeWeaponComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var hisGrace, out var melee, out var xform))
        {
            UpdateHisGrace(uid, hisGrace, melee, xform);
        }
    }

    private void UpdateHisGrace(EntityUid uid, HisGraceComponent hisGrace, MeleeWeaponComponent melee, TransformComponent xform)
    {
        if (hisGrace.CurrentState is HisGraceState.Dormant or HisGraceState.Death or HisGraceState.Ascended)
            return;

        if (TerminatingOrDeleted(uid) || hisGrace.User is not { } user)
        {
            hisGrace.CurrentState = HisGraceState.Dormant;
            return;
        }

        HandleUserDistance(uid, hisGrace, user);
        HandleGroundAttacks(uid, hisGrace, melee, xform);
        UpdateHungerState(hisGrace);
        ProcessHungerTick(hisGrace, user);
    }

    private void HandleUserDistance(EntityUid uid, HisGraceComponent hisGrace, EntityUid user)
    {
        if (_timing.CurTime <= hisGrace.NextUserAttack
        || _lookup.GetEntitiesInRange(uid, 1f).Contains(user))
            return;

        var popUp = Loc.GetString("hisgrace-too-far");
        _popup.PopupEntity(popUp, user, user, PopupType.LargeCaution);
        _damageable.TryChangeDamage(user, hisGrace.BaseDamage, targetPart: TargetBodyPart.Head, ignoreResistances: true);

        hisGrace.NextUserAttack = _timing.CurTime + hisGrace.TickDelay;
    }

    private void HandleGroundAttacks(EntityUid uid, HisGraceComponent hisGrace, MeleeWeaponComponent melee, TransformComponent xform)
    {
        if (hisGrace.IsHeld && hisGrace.Holder == hisGrace.User
            || _timing.CurTime < hisGrace.NextGroundAttack)
            return;

        var nearbyEnts = _lookup.GetEntitiesInRange(uid, 1f);

        foreach (var entity in nearbyEnts.Where(entity => HasComp<MobStateComponent>(entity)
        && entity != hisGrace.User
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

    private void UpdateHungerState(HisGraceComponent hisGrace)
    {
        var downgradeNeeded =
        hisGrace.StateThresholds.TryGetValue(hisGrace.CurrentState, out var currentThreshold)
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
    }

    private void ProcessHungerTick(HisGraceComponent hisGrace, EntityUid user)
    {
        if (hisGrace.NextHungerTick > _timing.CurTime)
            return;

        _damageable.TryChangeDamage(user, hisGrace.Healing);

        hisGrace.Hunger += hisGrace.HungerIncrement;
        hisGrace.NextHungerTick = _timing.CurTime + hisGrace.TickDelay;
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

        _chat.DispatchGlobalAnnouncement(Loc.GetString("hisgrace-ascension-announcement"), Name(user), true, comp.AscendSound, Color.PaleGoldenrod);
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
        if (!_threshold.TryGetThresholdForState(target, MobState.Critical, out var criticalThreshold))
            return comp.HungerOnDevourDefault;

        // hunger value is equal to the mutiplier times the crit threshold.
        // this is twenty for humans
        return (FixedPoint2)(comp.HungerOnDevourMultiplier * criticalThreshold);
    }

    private void SetUnremovable(EntityUid uid, bool enabled)
    {
        if (enabled)
        {
            EnsureComp<UnremoveableComponent>(uid);
            EnsureComp<JitteringComponent>(uid);
        }
        else
        {
            RemComp<UnremoveableComponent>(uid);
            RemComp<JitteringComponent>(uid);
        }
    }

    private void ReleaseContainedEntities(HisGraceComponent hisGrace)
    {
        var toRelease = hisGrace.Stomach.ContainedEntities;
        foreach (var ent in toRelease)
        {
            _containerSystem.TryRemoveFromContainer(ent);
            _stun.TryParalyze(ent, TimeSpan.FromSeconds(8), true);
        }
    }

    #endregion

}
