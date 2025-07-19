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
        SubscribeLocalEvent<SprinterComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<SprinterComponent, KnockedDownEvent>(OnKnockedDown);
        SubscribeLocalEvent<SprinterComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<SprinterComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<SprinterComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<SprinterComponent, SprintToggleEvent>(OnSprintToggle);
        SubscribeLocalEvent<SprinterComponent, ToggleWalkEvent>(OnToggleWalk);
    }

    #region Core Functions

    private sealed class SprintInputCmdHandler : InputCmdHandler
    {
        private SharedSprintingSystem _system;

        public SprintInputCmdHandler(SharedSprintingSystem system)
        {
            _system = system;
        }

        public override bool HandleCmdMessage(IEntityManager entManager, ICommonSession? session, IFullInputCmdMessage message)
        {
            if (session?.AttachedEntity == null) return false;

            _system.HandleSprintInput(session, message);
            return false;
        }
    }
    private void OnRefreshSpeed(Entity<SprinterComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.IsSprinting)
            args.ModifySpeed(1.5f);
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
        if (isSprinting
            && (!CanSprint(uid, component)
            || _timing.CurTime - component.LastSprint < component.TimeBetweenSprints))
            return;

        if (isSprinting != component.IsSprinting)
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
        _staminaSystem.ToggleStaminaDrain(uid, component.StaminaDrainRate, isSprinting, false, "sprint");
        Dirty(uid, component);
    }

    #endregion

    #region Conditionals

    private bool CanSprint(EntityUid uid, SprinterComponent component)
    {
        if (TryComp<CuffableComponent>(uid, out var cuffableComp) && !cuffableComp.CanStillInteract)
        {
            _popupSystem.PopupClient(Loc.GetString("no-sprint-while-restrained"), uid, uid, PopupType.Medium);
            return false;
        }

        if (HasComp<StandingStateComponent>(uid) && _standing.IsDown(uid))
        {
            _popupSystem.PopupClient(Loc.GetString("no-sprint-while-lying"), uid, uid, PopupType.Medium);
            return false;
        }

        if (_gravity.IsWeightless(uid))
        {
            _popupSystem.PopupClient(Loc.GetString("no-sprint-while-weightless"), uid, uid, PopupType.Medium);
            return false;
        }

        return true;
    }

    private void OnMobStateChangedEvent(EntityUid uid, SprinterComponent component, MobStateChangedEvent args)
    {
        if (!component.IsSprinting
            || args.NewMobState is MobState.Critical or MobState.Dead)
            return;

        ToggleSprint(args.Target, component, false, gracefulStop: false);
    }

    private void OnKnockedDown(EntityUid uid, SprinterComponent component, ref KnockedDownEvent args)
    {
        if (!component.IsSprinting)
            return;

        ToggleSprint(uid, component, false, gracefulStop: false);
    }

    private void OnStunned(EntityUid uid, SprinterComponent component, ref StunnedEvent args)
    {
        if (!component.IsSprinting)
            return;

        ToggleSprint(uid, component, false, gracefulStop: false);
    }

    private void OnDowned(EntityUid uid, SprinterComponent component, ref DownedEvent args)
    {
        if (!component.IsSprinting)
            return;

        ToggleSprint(uid, component, false, gracefulStop: false);
    }

    private void OnSleep(EntityUid uid, SprinterComponent component, ref SleepStateChangedEvent args)
    {
        if (!component.IsSprinting
            || !args.FellAsleep)
            return;

        ToggleSprint(uid, component, false, gracefulStop: false);

        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;

        Dirty(uid, stamina);
    }

    private void OnToggleWalk(EntityUid uid, SprinterComponent component, ref ToggleWalkEvent args)
    {
        if (!component.IsSprinting)
            return;

        ToggleSprint(uid, component, false, gracefulStop: false);
    }

    #endregion
}
