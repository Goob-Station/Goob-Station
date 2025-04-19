// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Overlays;
using Content.Server.Atmos.Components;
using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._vg.TileMovement;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
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

public sealed partial class HisGraceSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _state = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    private ProtoId<DamageModifierSetPrototype> _ascensionDamageSet = new("HisGraceAscended");

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
    }

    private void OnInit(EntityUid uid, HisGraceComponent component, MapInitEvent args)
    {
        component.Stomach = _containerSystem.EnsureContainer<Container>(uid, "stomach");

        if (!TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        component.BaseDamage = melee.Damage;
    }

    private void OnEquipped(EntityUid uid, HisGraceComponent component, ref GotEquippedHandEvent args)
    {
        component.IsHeld = true;
    }

    private void OnUnequipped(EntityUid uid, HisGraceComponent component, ref GotUnequippedHandEvent args)
    {
        component.IsHeld = false;
    }

    private void OnMeleeHit(EntityUid uid, HisGraceComponent comp, ref MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
            TryDevour(comp, hitEntity);
    }

    private void OnUse(EntityUid uid, HisGraceComponent comp, ref UseInHandEvent args)
    {
        if (comp.CurrentState != HisGraceState.Dormant)
            return;

        comp.User = args.User;

        var popUp = Loc.GetString("hisgrace-use-start");
        _popup.PopupEntity(popUp, args.User, args.User, PopupType.MediumCaution);

        ChangeState(comp, HisGraceState.Peckish);
    }

    private void OnEntityConsumed(EntityUid uid, HisGraceComponent comp, ref HisGraceEntityConsumedEvent args)
    {
        comp.EntitiesAbsorbed++;
        comp.Hunger -= Math.Clamp(comp.HungerOnDevour, 0, 200);

        if (comp.EntitiesAbsorbed >= 25)
            ChangeState(comp, HisGraceState.Ascended);

        if (!TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        // 5 blunt per entity consumed
        comp.CurrentDamageIncrease.DamageDict["Blunt"] = comp.EntitiesAbsorbed * 5;
        melee.Damage = comp.BaseDamage + comp.CurrentDamageIncrease;
    }

    private void OnHungerChanged(EntityUid uid, HisGraceComponent comp, ref HisGraceHungerChangedEvent args)
    {
        if (args.NewState == HisGraceState.Ascended && TryComp<AppearanceComponent>(uid, out var appearanceComponent)) // add more logic here later
        {
            EnsureComp<UnremoveableComponent>(uid);
            DoAscension(comp);
            _appearance.SetData(uid, ToggleVisuals.Toggled, true, appearanceComponent);
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

                break;
            }

            case HisGraceState.Ravenous:
            case HisGraceState.Starving:
            {
                EnsureComp<UnremoveableComponent>(uid);
                EnsureComp<JitteringComponent>(uid);
                break;
            }

            case HisGraceState.Death:
            {
                if (comp.User is { } user)
                {
                    _damageable.TryChangeDamage(user, comp.DamageOnFail, targetPart: TargetBodyPart.Head,  origin: uid, ignoreResistances: true);

                    var popUp = Loc.GetString("hisgrace-death", ("target", Name(user)));
                    _popup.PopupEntity(popUp, user, user, PopupType.LargeCaution);
                }


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
                continue;

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
            if (!hisGrace.IsHeld)
            {
                foreach (var entity in nearbyEnts.Where(entity => HasComp<MobStateComponent>(entity) && entity != hisGrace.User
                             &&  _timing.CurTime > hisGrace.NextGroundAttack
                             && !TerminatingOrDeleted(entity)
                             && !_containerSystem.IsEntityOrParentInContainer(entity)))
                {
                    var coordinates = _transform.GetMapCoordinates(uid);
                    var angle = _transform.GetRelativePosition(xform, entity, GetEntityQuery<TransformComponent>()).ToAngle();

                    _damageable.TryChangeDamage(entity, melee.Damage);
                    _audio.PlayPvs(melee.HitSound, uid);
                    _popup.PopupEntity(Loc.GetString("hisgrace-attack-popup", ("target", Name(entity))), uid, PopupType.LargeCaution);
                    _melee.DoLunge(uid, uid, angle, coordinates.Position, null, angle, false, false);

                    TryDevour(hisGrace, entity);
                    hisGrace.NextGroundAttack = _timing.CurTime + hisGrace.TickDelay;

                    break;
                }
            }

            var orderedThresholds = hisGrace.StateThresholds
                .OrderBy(t => t.Value.Threshold) // Order thresholds ascending
                .ToList();

            // Update hunger based on threshold.
            for (var i = orderedThresholds.Count - 1; i >= 0; i--)
            {
                var threshold = orderedThresholds[i];
                if (hisGrace.Hunger < threshold.Value.Threshold)
                    continue;

                if (threshold.Key <= hisGrace.CurrentState)
                    break;

                hisGrace.HungerIncrement = threshold.Value.Increment;
                ChangeState(hisGrace, threshold.Key);
                break;
            }

            if (hisGrace.StateThresholds.TryGetValue(hisGrace.CurrentState, out var currentThreshold) && hisGrace.Hunger < currentThreshold.Threshold)
            {
                // Find the highest threshold below the current hunger
                foreach (var threshold in orderedThresholds.Where(threshold => hisGrace.Hunger >= threshold.Value.Threshold))
                {
                    hisGrace.HungerIncrement = threshold.Value.Increment;

                    ChangeState(hisGrace, threshold.Key);
                    break;
                }
            }

            if (hisGrace.NextHungerTick > _timing.CurTime)
                continue;

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

        _damageable.SetDamageModifierSetId(user, _ascensionDamageSet);

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
        if (!_state.IsIncapacitated(target) || comp.CurrentState == HisGraceState.Dormant || !_containerSystem.Insert(target, comp.Stomach))
            return false;

        var devourPopup = Loc.GetString("hisgrace-devour", ("target", Name(target)));
        _audio.PlayPvs(comp.SoundDevour, target);
        _popup.PopupEntity(devourPopup, target, PopupType.LargeCaution);

        // don't apply bonuses for enities consumed that don't have minds
        if (_mind.TryGetMind(target, out _, out _))
        {
            var ev = new HisGraceEntityConsumedEvent();
            RaiseLocalEvent(comp.Owner, ref ev);
        }

        return true;
    }

    #endregion

}
