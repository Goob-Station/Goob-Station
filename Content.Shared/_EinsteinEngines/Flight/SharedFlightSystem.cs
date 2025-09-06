// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.Climbing.Events;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Physics;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;


namespace Content.Shared._EinsteinEngines.Flight;
public abstract class SharedFlightSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlightComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlightComponent, ToggleFlightEvent>(OnToggleFlight);
        SubscribeLocalEvent<FlightComponent, RefreshWeightlessModifiersEvent>(OnRefreshWeightlessMoveSpeed);
        SubscribeLocalEvent<FlightComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<CuffableComponent, FlightAttemptEvent>(OnCuffableFlightAttempt);
        SubscribeLocalEvent<StandingStateComponent, FlightAttemptEvent>(OnStandingStateFlightAttempt);
        SubscribeLocalEvent<ZombieComponent, FlightAttemptEvent>(OnZombieFlightAttempt);
        SubscribeLocalEvent<FlightComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<FlightComponent, EntityZombifiedEvent>(OnFlightDisablingEvent);
        SubscribeLocalEvent<FlightComponent, KnockedDownEvent>(OnFlightDisablingEvent);
        SubscribeLocalEvent<FlightComponent, StunnedEvent>(OnFlightDisablingEvent);
        SubscribeLocalEvent<FlightComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<FlightComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<FlightComponent, AttemptClimbEvent>(OnAttemptClimb);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FlightComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.On)
                continue;

            component.TimeUntilFlap -= frameTime;

            if (component.TimeUntilFlap <= 0f)
            {
                _audio.PlayPredicted(component.FlapSound, uid, uid);
                component.TimeUntilFlap = component.FlapInterval;
            }

            // We make it 0.7f to compensate by how comparatively lame it is vs sprinting while on stimulants as another species.
            if (TryComp<StaminaModifierComponent>(uid, out var staminaComp))
                _staminaSystem.ModifyStaminaDrain(uid,
                    component.StaminaDrainKey,
                    component.StaminaDrainRate * staminaComp.Modifier * component.StaminaDrainMultiplier);
        }
    }

    #region Core Functions
    private void OnStartup(EntityUid uid, FlightComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnShutdown(EntityUid uid, FlightComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ToggleActionEntity);
        if (!TerminatingOrDeleted(uid))
            ToggleActive(uid, false, component);
    }

    public void ToggleActive(EntityUid uid, bool active, FlightComponent component, bool gracefulStop = true)
    {
        component.On = active;
        component.TimeUntilFlap = 0f;
        _actionsSystem.SetToggled(component.ToggleActionEntity, component.On);
        RaiseLocalEvent(uid, new FlightEvent(uid, component.On, component.IsAnimated));
        _staminaSystem.ToggleStaminaDrain(uid, component.StaminaDrainRate, active, false, component.StaminaDrainKey, uid);
        _movementSpeed.RefreshWeightlessModifiers(uid);
        ToggleCollisionMasks(uid, component);
        UpdateHands(uid, active);

        if (component.CanFail && !gracefulStop)
            _damageable.TryChangeDamage(uid, component.FailDamageSpecifier);

        Dirty(uid, component);
    }

    private void OnToggleFlight(EntityUid uid, FlightComponent component, ToggleFlightEvent args)
    {
        if (!component.On
            && !CanFly(uid, component))
            return;

        ToggleActive(uid, !component.On, component);
    }

    private void ToggleCollisionMasks(EntityUid uid, FlightComponent component)
    {
        if (component.On)
            DisableCollisionMasks(uid, component);
        else
            EnableCollisionMasks(uid, component);
    }

    private void DisableCollisionMasks(EntityUid uid, FlightComponent component)
    {
        if (!component.On)
            return;

        if (TryComp(uid, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, fixture) in fixtureComponent.Fixtures)
            {
                var newMask = (fixture.CollisionMask
                    & (int) ~CollisionGroup.HighImpassable
                    & (int) ~CollisionGroup.MidImpassable)
                    | (int) CollisionGroup.InteractImpassable;

                if (fixture.CollisionMask == newMask)
                    continue;

                component.ChangedFixtures.Add((key, fixture.CollisionMask));
                _physics.SetCollisionMask(uid,
                    key,
                    fixture,
                    newMask,
                    manager: fixtureComponent);
            }
        }
        return;
    }

    private void EnableCollisionMasks(EntityUid uid, FlightComponent component)
    {
        if (component.On)
            return;

        // Restore normal collision masks
        if (TryComp(uid, out FixturesComponent? fixtureComponent))
            foreach (var (key, originalMask) in component.ChangedFixtures)
                if (fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                    _physics.SetCollisionMask(uid, key, fixture, originalMask, fixtureComponent);

        component.ChangedFixtures.Clear();
    }

    private void UpdateHands(EntityUid uid, bool flying)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent))
            return;

        if (flying)
            BlockHands(uid, handsComponent);
        else
            FreeHands(uid);
    }

    private void BlockHands(EntityUid uid, HandsComponent handsComponent)
    {
        var freeHands = 0;
        foreach (var hand in _hands.EnumerateHands((uid, handsComponent)))
        {
            if (!_hands.TryGetHeldItem((uid, handsComponent), hand, out var held))
            {
                freeHands++;
                continue;
            }

            // Is this entity removable? (they might have handcuffs on)
            if (HasComp<UnremoveableComponent>(held) && held != uid)
                continue;

            _hands.DoDrop((uid, handsComponent), hand);
            freeHands++;
            if (freeHands == 2)
                break;
        }
        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem1))
            EnsureComp<UnremoveableComponent>(virtItem1.Value);

        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem2))
            EnsureComp<UnremoveableComponent>(virtItem2.Value);
    }

    private void FreeHands(EntityUid uid) => _virtualItem.DeleteInHandsMatching(uid, uid);

    private void OnRefreshWeightlessMoveSpeed(EntityUid uid, FlightComponent component, ref RefreshWeightlessModifiersEvent args)
    {
        if (!component.On)
            return;

        args.ModifyAcceleration(component.SpeedModifier);
        args.ModifyFriction(component.FrictionModifier, component.FrictionNoInputModifier);
    }

    private void OnBeforeStaminaDamage(EntityUid uid, FlightComponent component, ref BeforeStaminaDamageEvent args)
    {
        if (!component.On
            || args.Value > 0)
            return;

        args.Value *= component.StaminaRegenMultiplier;
    }

    #endregion

    #region Conditionals

    private bool CanFly(EntityUid uid, FlightComponent component)
    {
        var ev = new FlightAttemptEvent();
        RaiseLocalEvent(uid, ref ev);

        return !ev.Cancelled;
    }

    private void OnCuffableFlightAttempt(EntityUid uid, CuffableComponent component, ref FlightAttemptEvent args)
    {
        if (component.CanStillInteract)
            return;

        _popupSystem.PopupClient(Loc.GetString("no-flight-while-restrained"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    private void OnZombieFlightAttempt(EntityUid uid, ZombieComponent component, ref FlightAttemptEvent args)
    {
        _popupSystem.PopupClient(Loc.GetString("no-flight-while-zombified"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    private void OnStandingStateFlightAttempt(EntityUid uid, StandingStateComponent component, ref FlightAttemptEvent args)
    {
        if (!_standing.IsDown(uid, component))
            return;

        _popupSystem.PopupClient(Loc.GetString("no-flight-while-lying"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    #endregion

    #region Misc.Handlers
    private void OnMobStateChangedEvent(EntityUid uid, FlightComponent component, MobStateChangedEvent args)
    {
        if (!component.On
            || args.NewMobState is MobState.Critical or MobState.Dead)
            return;

        ToggleActive(args.Target, false, component, gracefulStop: false);
    }
    private void OnSleep(EntityUid uid, FlightComponent component, ref SleepStateChangedEvent args)
    {
        if (!component.On
            || !args.FellAsleep)
            return;

        ToggleActive(uid, false, component, gracefulStop: false);
    }

    private void OnDowned(EntityUid uid, FlightComponent component, ref DownedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component, gracefulStop: false);
        // We need this crap because standingsys only raises shit on server lmao
        RaiseNetworkEvent(new ToggleFlightVisualsEvent(GetNetEntity(uid), false, component.IsAnimated));
    }

    private void OnFlightDisablingEvent<T>(EntityUid uid, FlightComponent component, ref T args) where T : notnull
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component, gracefulStop: false);
    }

    private void OnAttemptClimb(EntityUid uid, FlightComponent component, AttemptClimbEvent args)
    {
        if (!component.On)
            return;

        args.Cancelled = true;
    }

    #endregion
}
public sealed partial class ToggleFlightEvent : InstantActionEvent { }
