// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Audio.Systems;


namespace Content.Shared._EinsteinEngines.Flight;
public abstract class SharedFlightSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlightComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlightComponent, ToggleFlightEvent>(OnToggleFlight);
        SubscribeLocalEvent<FlightComponent, FlightDoAfterEvent>(OnFlightDoAfter);
        SubscribeLocalEvent<FlightComponent, RefreshWeightlessModifiersEvent>(OnRefreshWeightlessMoveSpeed);
        SubscribeLocalEvent<FlightComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<FlightComponent, EntityZombifiedEvent>(OnZombified);
        SubscribeLocalEvent<FlightComponent, KnockedDownEvent>(OnKnockedDown);
        SubscribeLocalEvent<FlightComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<FlightComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<FlightComponent, SleepStateChangedEvent>(OnSleep);
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

            if (component.TimeUntilFlap > 0f)
                continue;

            _audio.PlayPredicted(component.FlapSound, uid, uid);
            component.TimeUntilFlap = component.FlapInterval;

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

    public void ToggleActive(EntityUid uid, bool active, FlightComponent component)
    {
        component.On = active;
        component.TimeUntilFlap = 0f;
        _actionsSystem.SetToggled(component.ToggleActionEntity, component.On);
        RaiseLocalEvent(uid, new FlightEvent(uid, component.On, component.IsAnimated));
        _staminaSystem.ToggleStaminaDrain(uid, component.StaminaDrainRate, active, false, "flight");
        _movementSpeed.RefreshWeightlessModifiers(uid);
        UpdateHands(uid, active);
        Dirty(uid, component);
    }

    private void OnToggleFlight(EntityUid uid, FlightComponent component, ToggleFlightEvent args)
    {
        // If the user isnt flying, we check for conditionals and initiate a doafter.
        if (!component.On)
        {
            if (!CanFly(uid, component))
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager,
            uid, component.ActivationDelay,
            new FlightDoAfterEvent(), uid, target: uid)
            {
                BlockDuplicate = true,
                BreakOnDamage = true,
                NeedHand = true,
                MultiplyDelay = false, // Goobstation
            };

            if (!_doAfter.TryStartDoAfter(doAfterArgs))
                return;
        }
        else
            ToggleActive(uid, false, component);
    }

    private void OnFlightDoAfter(EntityUid uid, FlightComponent component, FlightDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        ToggleActive(uid, true, component);
        args.Handled = true;
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
        foreach (var hand in _hands.EnumerateHands(uid, handsComponent))
        {
            if (hand.HeldEntity == null)
            {
                freeHands++;
                continue;
            }

            // Is this entity removable? (they might have handcuffs on)
            if (HasComp<UnremoveableComponent>(hand.HeldEntity) && hand.HeldEntity != uid)
                continue;

            _hands.DoDrop(uid, hand, true, handsComponent);
            freeHands++;
            if (freeHands == 2)
                break;
        }
        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem1))
            EnsureComp<UnremoveableComponent>(virtItem1.Value);

        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem2))
            EnsureComp<UnremoveableComponent>(virtItem2.Value);
    }

    private void FreeHands(EntityUid uid)
    {
        _virtualItem.DeleteInHandsMatching(uid, uid);
    }

    private void OnRefreshWeightlessMoveSpeed(EntityUid uid, FlightComponent component, ref RefreshWeightlessModifiersEvent args)
    {
        if (!component.On)
            return;

        args.ModifyAcceleration(component.SpeedModifier);
    }

    #endregion

    #region Conditionals

    private bool CanFly(EntityUid uid, FlightComponent component)
    {
        if (TryComp<CuffableComponent>(uid, out var cuffableComp) && !cuffableComp.CanStillInteract)
        {
            _popupSystem.PopupClient(Loc.GetString("no-flight-while-restrained"), uid, uid, PopupType.Medium);
            return false;
        }

        if (HasComp<ZombieComponent>(uid))
        {
            _popupSystem.PopupClient(Loc.GetString("no-flight-while-zombified"), uid, uid, PopupType.Medium);
            return false;
        }

        if (HasComp<StandingStateComponent>(uid) && _standing.IsDown(uid))
        {
            _popupSystem.PopupClient(Loc.GetString("no-flight-while-lying"), uid, uid, PopupType.Medium);
            return false;
        }
        return true;
    }
    private void OnMobStateChangedEvent(EntityUid uid, FlightComponent component, MobStateChangedEvent args)
    {
        if (!component.On
            || args.NewMobState is MobState.Critical or MobState.Dead)
            return;

        ToggleActive(args.Target, false, component);
    }

    private void OnZombified(EntityUid uid, FlightComponent component, ref EntityZombifiedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(args.Target, false, component);
        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;
        Dirty(uid, stamina);
    }

    private void OnKnockedDown(EntityUid uid, FlightComponent component, ref KnockedDownEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component);
    }

    private void OnStunned(EntityUid uid, FlightComponent component, ref StunnedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component);
    }

    private void OnDowned(EntityUid uid, FlightComponent component, ref DownedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component);
        // We need this crap because standingsys only raises shit on server lmao
        RaiseNetworkEvent(new ToggleFlightVisualsEvent(GetNetEntity(uid), false, component.IsAnimated));
    }

    private void OnSleep(EntityUid uid, FlightComponent component, ref SleepStateChangedEvent args)
    {
        if (!component.On
            || !args.FellAsleep)
            return;

        ToggleActive(uid, false, component);
        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;

        Dirty(uid, stamina);
    }

    #endregion
}
public sealed partial class ToggleFlightEvent : InstantActionEvent { }
