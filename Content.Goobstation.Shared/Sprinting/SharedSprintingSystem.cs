// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Movement;
using Content.Shared._EinsteinEngines.Flight;
using Content.Shared.Bed.Sleep;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Gravity;
using Content.Shared.Input;
using Content.Shared.Mobs;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Goobstation.Shared.Sprinting;
public abstract class SharedSprintingSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedMoverController _moverController = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<SprinterComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Sprint, new SprintInputCmdHandler(this))
            .Register<SharedSprintingSystem>();
        SubscribeLocalEvent<SprinterComponent, SprintToggleEvent>(OnSprintToggle);
        SubscribeLocalEvent<SprinterComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<SprinterComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<SprinterComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<SprinterComponent, ToggleWalkEvent>(OnToggleWalk);
        SubscribeLocalEvent<SprinterComponent, KnockedDownEvent>(OnSprintDisablingEvent);
        SubscribeLocalEvent<SprinterComponent, StunnedEvent>(OnSprintDisablingEvent);
        SubscribeLocalEvent<SprinterComponent, DownedEvent>(OnSprintDisablingEvent);
        SubscribeLocalEvent<CuffableComponent, SprintAttemptEvent>(OnCuffableSprintAttempt);
        SubscribeLocalEvent<StandingStateComponent, SprintAttemptEvent>(OnStandingStateSprintAttempt);
        SubscribeLocalEvent<SprinterComponent, EntityZombifiedEvent>(OnZombified);
    }

    #region Core Functions

    private sealed class SprintInputCmdHandler(SharedSprintingSystem system) : InputCmdHandler
    {
        public override bool HandleCmdMessage(IEntityManager entManager, ICommonSession? session, IFullInputCmdMessage message)
        {
            if (session?.AttachedEntity == null)
                return false;

            system.HandleSprintInput(session, message);
            return false;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // We dont add it to the EQE since the comp might get added as this runs.
        var query = EntityQueryEnumerator<SprinterComponent, StaminaModifierComponent>();
        while (query.MoveNext(out var uid, out var sprinterComp, out var staminaComp))
        {
            if (!sprinterComp.IsSprinting
                || !sprinterComp.ScaleWithStamina
                || staminaComp.Modifier <= 1f)
                continue;

            _staminaSystem.ModifyStaminaDrain(uid,
                sprinterComp.StaminaDrainKey,
                sprinterComp.StaminaDrainRate * staminaComp.Modifier * sprinterComp.StaminaDrainMultiplier);
        }
    }

    private void OnRefreshSpeed(Entity<SprinterComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        args.ModifySpeed(ent.Comp.SprintSpeedMultiplier);
    }

    private void HandleSprintInput(ICommonSession? session, IFullInputCmdMessage message)
    {
        if (session?.AttachedEntity == null
            || !TryComp<SprinterComponent>(session.AttachedEntity, out var sprinterComponent)
            || !TryComp<InputMoverComponent>(session.AttachedEntity, out var inputMoverComponent)
            || !sprinterComponent.IsSprinting
            // We check this instead of physics so that we can gatekeep sprinting to only work when you are moving intentionally, and not walking.
            && _moverController.GetVelocityInput(inputMoverComponent).Sprinting == Vector2.Zero)
            return;

        if (!sprinterComponent.CanSprint)
        {
            if (message.State == BoundKeyState.Down) // Without this check the message triggers when holding and releasing.
                _popupSystem.PopupClient(Loc.GetString("sprint-disabled"), session.AttachedEntity.Value, session.AttachedEntity.Value, PopupType.Medium);

            return;
        }

        RaiseLocalEvent(session.AttachedEntity.Value, new SprintToggleEvent(!sprinterComponent.IsSprinting && message.State == BoundKeyState.Down));
    }

    private void OnSprintToggle(EntityUid uid, SprinterComponent component, ref SprintToggleEvent args) =>
        ToggleSprint(uid, component, args.IsSprinting);

    private void ToggleSprint(EntityUid uid, SprinterComponent component, bool isSprinting, bool gracefulStop = true)
    {
        // Breaking these into two separate if's for better readability
        if (isSprinting == component.IsSprinting)
            return;


        if (isSprinting
            && (!CanSprint(uid, component)
            || _timing.CurTime - component.LastSprint < component.TimeBetweenSprints))
            return;

        component.LastSprint = _timing.CurTime;
        component.IsSprinting = isSprinting;

        if (isSprinting)
        {
            RaiseLocalEvent(uid, new SprintStartEvent());
            _audio.PlayPredicted(component.SprintStartupSound, uid, uid);
        }

        if (!gracefulStop)
            _damageable.TryChangeDamage(uid, component.SprintDamageSpecifier);

        _movementSpeed.RefreshMovementSpeedModifiers(uid);
        _staminaSystem.ToggleStaminaDrain(uid, component.StaminaDrainRate, isSprinting, true, component.StaminaDrainKey);
        Dirty(uid, component);
    }

    #endregion

    #region Conditionals

    private bool CanSprint(EntityUid uid, SprinterComponent component)
    {
        // Awaiting on a wizden PR that refactors gravity from whatever the fuck this is.
        if (_gravity.IsWeightless(uid))
        {
            _popupSystem.PopupClient(Loc.GetString("no-sprint-while-weightless"), uid, uid, PopupType.Medium);
            return false;
        }

        var ev = new SprintAttemptEvent();
        RaiseLocalEvent(uid, ref ev);

        return !ev.Cancelled;
    }

    private void OnCuffableSprintAttempt(EntityUid uid, CuffableComponent component, ref SprintAttemptEvent args)
    {
        if (component.CanStillInteract)
            return;

        _popupSystem.PopupClient(Loc.GetString("no-sprint-while-restrained"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    private void OnStandingStateSprintAttempt(EntityUid uid, StandingStateComponent component, ref SprintAttemptEvent args)
    {
        if (!_standing.IsDown(uid, component))
            return;

        _popupSystem.PopupClient(Loc.GetString("no-sprint-while-lying"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    #endregion

    #region Misc.Handlers
    private void OnBeforeStaminaDamage(EntityUid uid, SprinterComponent component, ref BeforeStaminaDamageEvent args)
    {
        if (!component.IsSprinting
            || args.Value > 0)
            return;

        args.Value *= component.StaminaRegenMultiplier;
    }

    private void OnMobStateChangedEvent(EntityUid uid, SprinterComponent component, MobStateChangedEvent args)
    {
        if (!component.IsSprinting
            || args.NewMobState is MobState.Critical or MobState.Dead)
            return;

        ToggleSprint(args.Target, component, false, gracefulStop: false);
    }

    private void OnSleep(EntityUid uid, SprinterComponent component, ref SleepStateChangedEvent args)
    {
        if (!component.IsSprinting
            || !args.FellAsleep)
            return;

        ToggleSprint(uid, component, false, gracefulStop: false);
    }

    private void OnToggleWalk(EntityUid uid, SprinterComponent component, ref ToggleWalkEvent args)
    {
        if (!component.IsSprinting)
            return;

        ToggleSprint(uid, component, false);
    }

    private void OnSprintDisablingEvent<T>(EntityUid uid, SprinterComponent component, ref T args) where T : notnull
    {
        if (!component.IsSprinting)
            return;

        ToggleSprint(uid, component, false, gracefulStop: false);
    }
    private void OnZombified(EntityUid uid, SprinterComponent component, ref EntityZombifiedEvent args) =>
        component.SprintSpeedMultiplier *= 0.5f; // We dont want super fast zombies do we?

    #endregion
}
