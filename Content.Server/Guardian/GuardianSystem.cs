// SPDX-FileCopyrightText: 2021 CrudeWax <75271456+CrudeWax@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Sirionaut <148076704+Sirionaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 No Elka <125199100+NoElkaTheGod@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared._Goobstation.Wizard.Guardian;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Guardian;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Guardian
{
    /// <summary>
    /// A guardian has a host it's attached to that it fights for. A fighting spirit.
    /// </summary>
    public sealed class GuardianSystem : EntitySystem
    {
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly DamageableSystem _damageSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly BodySystem _bodySystem = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly NpcFactionSystem _faction = default!; // Goobstation

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GuardianCreatorComponent, UseInHandEvent>(OnCreatorUse);
            SubscribeLocalEvent<GuardianCreatorComponent, AfterInteractEvent>(OnCreatorInteract);
            SubscribeLocalEvent<GuardianCreatorComponent, ExaminedEvent>(OnCreatorExamine);
            SubscribeLocalEvent<GuardianCreatorComponent, GuardianCreatorDoAfterEvent>(OnDoAfter);

            SubscribeLocalEvent<GuardianComponent, ComponentShutdown>(OnGuardianShutdown);
            SubscribeLocalEvent<GuardianComponent, MoveEvent>(OnGuardianMove);
            SubscribeLocalEvent<GuardianComponent, DamageChangedEvent>(OnGuardianDamaged);
            SubscribeLocalEvent<GuardianComponent, PlayerAttachedEvent>(OnGuardianPlayerAttached);
            SubscribeLocalEvent<GuardianComponent, PlayerDetachedEvent>(OnGuardianPlayerDetached);

            SubscribeLocalEvent<GuardianHostComponent, ComponentInit>(OnHostInit);
            SubscribeLocalEvent<GuardianHostComponent, MoveEvent>(OnHostMove);
            SubscribeLocalEvent<GuardianHostComponent, MobStateChangedEvent>(OnHostStateChange);
            SubscribeLocalEvent<GuardianHostComponent, ComponentShutdown>(OnHostShutdown);

            SubscribeLocalEvent<GuardianHostComponent, GuardianToggleActionEvent>(OnPerformAction);

            SubscribeLocalEvent<GuardianComponent, AttackAttemptEvent>(OnGuardianAttackAttempt);
        }

        private void OnGuardianShutdown(EntityUid uid, GuardianComponent component, ComponentShutdown args)
        {
            if (!TerminatingOrDeleted(uid)) // Goobstation
                RemCompDeferred<GuardianSharedComponent>(uid);

            var host = component.Host;
            component.Host = null;

            if (!TryComp(host, out GuardianHostComponent? hostComponent))
                return;

            _container.Remove(uid, hostComponent.GuardianContainer);
            hostComponent.HostedGuardian = null;
            Dirty(host.Value, hostComponent);
            QueueDel(hostComponent.ActionEntity);
            hostComponent.ActionEntity = null;
        }

        private void OnPerformAction(EntityUid uid, GuardianHostComponent component, GuardianToggleActionEvent args)
        {
            if (args.Handled)
                return;

            if (_container.IsEntityInContainer(uid))
            {
                _popupSystem.PopupEntity(Loc.GetString("guardian-inside-container"), uid, uid);
                return;
            }

            if (component.HostedGuardian != null)
                ToggleGuardian(uid, component);

            args.Handled = true;
        }

        private void OnGuardianPlayerDetached(EntityUid uid, GuardianComponent component, PlayerDetachedEvent args)
        {
            var host = component.Host;
            if (!TryComp<GuardianHostComponent>(host, out var hostComponent) || TerminatingOrDeleted(host.Value))
            {
                QueueDel(uid);
                return;
            }

            RetractGuardian(host.Value, hostComponent, uid, component);
        }

        private void OnGuardianPlayerAttached(EntityUid uid, GuardianComponent component, PlayerAttachedEvent args)
        {
            var host = component.Host;

            if (!HasComp<GuardianHostComponent>(host))
            {
                QueueDel(uid);
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("guardian-available"), host.Value, host.Value);
        }

        private void OnHostInit(EntityUid uid, GuardianHostComponent component, ComponentInit args)
        {
            component.GuardianContainer = _container.EnsureContainer<ContainerSlot>(uid, "GuardianContainer");
            _actionSystem.AddAction(uid, ref component.ActionEntity, component.Action);
        }

        private void OnHostShutdown(EntityUid uid, GuardianHostComponent component, ComponentShutdown args)
        {
            if (component.HostedGuardian is not {} guardian)
                return;

            // Ensure held items are dropped before deleting guardian.
            if (HasComp<HandsComponent>(guardian))
                _bodySystem.GibBody(component.HostedGuardian.Value);

            QueueDel(guardian);
            QueueDel(component.ActionEntity);
            component.ActionEntity = null;
        }

        private void OnGuardianAttackAttempt(EntityUid uid, GuardianComponent component, AttackAttemptEvent args)
        {
            if (args.Cancelled || args.Target != component.Host)
                return;

            // why is this server side code? This should be in shared
            _popupSystem.PopupCursor(Loc.GetString("guardian-attack-host"), uid, PopupType.LargeCaution);
            args.Cancel();
        }

        public void ToggleGuardian(EntityUid user, GuardianHostComponent hostComponent)
        {
            if (!TryComp<GuardianComponent>(hostComponent.HostedGuardian, out var guardianComponent))
                return;

            if (guardianComponent.GuardianLoose)
                RetractGuardian(user, hostComponent, hostComponent.HostedGuardian.Value, guardianComponent);
            else
                ReleaseGuardian(user, hostComponent, hostComponent.HostedGuardian.Value, guardianComponent);
        }

        /// <summary>
        /// Adds the guardian host component to the user and spawns the guardian inside said component
        /// </summary>
        private void OnCreatorUse(EntityUid uid, GuardianCreatorComponent component, UseInHandEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            UseCreator(args.User, args.User, uid, component);
        }

        private void OnCreatorInteract(EntityUid uid, GuardianCreatorComponent component, AfterInteractEvent args)
        {
            if (args.Handled || args.Target == null || !args.CanReach)
                return;

            args.Handled = true;
            UseCreator(args.User, args.Target.Value, uid, component);
        }
        private void UseCreator(EntityUid user, EntityUid target, EntityUid injector, GuardianCreatorComponent component)
        {
            if (component.Used)
            {
                _popupSystem.PopupEntity(Loc.GetString("guardian-activator-empty-invalid-creation"), user, user);
                return;
            }

            // Can only inject things with the component...
            if (!HasComp<CanHostGuardianComponent>(target))
            {
                _popupSystem.PopupEntity(Loc.GetString("guardian-activator-invalid-target"), user, user);
                return;
            }

            // If user is already a host don't duplicate.
            if (HasComp<GuardianHostComponent>(target))
            {
                _popupSystem.PopupEntity(Loc.GetString("guardian-already-present-invalid-creation"), user, user);
                return;
            }

            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, user, component.InjectionDelay, new GuardianCreatorDoAfterEvent(), injector, target: target, used: injector){BreakOnMove = true});
        }

        private void OnDoAfter(EntityUid uid, GuardianCreatorComponent component, DoAfterEvent args)
        {
            if (args.Handled || args.Args.Target == null)
                return;

            if (args.Cancelled || component.Deleted || component.Used || !_handsSystem.IsHolding(args.Args.User, uid, out _) || HasComp<GuardianHostComponent>(args.Args.Target))
                return;

            var hostXform = Transform(args.Args.Target.Value);
            var host = EnsureComp<GuardianHostComponent>(args.Args.Target.Value);
            // Use map position so it's not inadvertantly parented to the host + if it's in a container it spawns outside I guess.
            var guardian = Spawn(component.GuardianProto, _transform.GetMapCoordinates(args.Args.Target.Value, xform: hostXform));

            // Goobstation start
            _faction.IgnoreEntity(guardian, args.Args.Target.Value);
            var sharedComp = EnsureComp<GuardianSharedComponent>(guardian);
            sharedComp.Host = args.Args.Target.Value;
            Dirty(guardian, sharedComp);
            // Goobstation end

            _container.Insert(guardian, host.GuardianContainer);
            host.HostedGuardian = guardian;

            if (TryComp<GuardianComponent>(guardian, out var guardianComp))
            {
                guardianComp.Host = args.Args.Target.Value;
                _audio.PlayPvs("/Audio/Effects/guardian_inject.ogg", args.Args.Target.Value);
                _popupSystem.PopupEntity(Loc.GetString("guardian-created"), args.Args.Target.Value, args.Args.Target.Value);
                // Exhaust the activator
                component.Used = true;
            }
            else
            {
                Log.Error($"Tried to spawn a guardian that doesn't have {nameof(GuardianComponent)}");
                QueueDel(guardian);
            }

            args.Handled = true;
        }

        /// <summary>
        /// Triggers when the host receives damage which puts the host in either critical or killed state
        /// </summary>
        private void OnHostStateChange(EntityUid uid, GuardianHostComponent component, MobStateChangedEvent args)
        {
            if (component.HostedGuardian == null)
                return;

            if (args.NewMobState == MobState.Critical)
            {
                _popupSystem.PopupEntity(Loc.GetString("guardian-host-critical-warn"), component.HostedGuardian.Value, component.HostedGuardian.Value);
                _audio.PlayPvs("/Audio/Effects/guardian_warn.ogg", component.HostedGuardian.Value);
            }
            else if (args.NewMobState == MobState.Dead)
            {
                //TODO: Replace WithVariation with datafield
                _audio.PlayPvs("/Audio/Voice/Human/malescream_guardian.ogg", uid, AudioParams.Default.WithVariation(0.20f));
                RemComp<GuardianHostComponent>(uid);
            }
        }

        /// <summary>
        /// Handles guardian receiving damage and splitting it with the host according to his defence percent
        /// </summary>
        private void OnGuardianDamaged(EntityUid uid, GuardianComponent component, DamageChangedEvent args)
        {
            if (args.DamageDelta == null || component.Host == null || component.DamageShare == 0)
                return;

            _damageSystem.TryChangeDamage(
                component.Host,
                args.DamageDelta * component.DamageShare,
                origin: args.Origin,
                ignoreResistances: true,
                interruptsDoAfters: false);
            _popupSystem.PopupEntity(Loc.GetString("guardian-entity-taking-damage"), component.Host.Value, component.Host.Value);

        }

        /// <summary>
        /// Triggers while trying to examine an activator to see if it's used
        /// </summary>
        private void OnCreatorExamine(EntityUid uid, GuardianCreatorComponent component, ExaminedEvent args)
        {
           if (component.Used)
               args.PushMarkup(Loc.GetString("guardian-activator-empty-examine"));
        }

        /// <summary>
        /// Called every time the host moves, to make sure the distance between the host and the guardian isn't too far
        /// </summary>
        private void OnHostMove(EntityUid uid, GuardianHostComponent component, ref MoveEvent args)
        {
            if (!TryComp(component.HostedGuardian, out GuardianComponent? guardianComponent) ||
                !guardianComponent.GuardianLoose)
            {
                return;
            }

            CheckGuardianMove(uid, component.HostedGuardian.Value, component);
        }

        /// <summary>
        /// Called every time the guardian moves: makes sure it's not out of it's allowed distance
        /// </summary>
        private void OnGuardianMove(EntityUid uid, GuardianComponent component, ref MoveEvent args)
        {
            if (!component.GuardianLoose || component.Host == null)
                return;

            CheckGuardianMove(component.Host.Value, uid, guardianComponent: component);
        }

        /// <summary>
        /// Retract the guardian if either the host or the guardian move away from each other.
        /// </summary>
        private void CheckGuardianMove(
            EntityUid hostUid,
            EntityUid guardianUid,
            GuardianHostComponent? hostComponent = null,
            GuardianComponent? guardianComponent = null,
            TransformComponent? hostXform = null,
            TransformComponent? guardianXform = null)
        {
            if (TerminatingOrDeleted(guardianUid) || TerminatingOrDeleted(hostUid))
                return;

            if (!Resolve(hostUid, ref hostComponent, ref hostXform) ||
                !Resolve(guardianUid, ref guardianComponent, ref guardianXform))
            {
                return;
            }

            if (!guardianComponent.GuardianLoose)
                return;

            if (!_transform.InRange(guardianXform.Coordinates, hostXform.Coordinates, guardianComponent.DistanceAllowed))
                RetractGuardian(hostUid, hostComponent, guardianUid, guardianComponent);
        }

        private void ReleaseGuardian(EntityUid host, GuardianHostComponent hostComponent, EntityUid guardian, GuardianComponent guardianComponent)
        {
            if (guardianComponent.GuardianLoose)
            {
                DebugTools.Assert(!hostComponent.GuardianContainer.Contains(guardian));
                return;
            }

            DebugTools.Assert(hostComponent.GuardianContainer.Contains(guardian));
            _container.Remove(guardian, hostComponent.GuardianContainer);
            DebugTools.Assert(!hostComponent.GuardianContainer.Contains(guardian));

            guardianComponent.GuardianLoose = true;
        }

        private void RetractGuardian(EntityUid host,GuardianHostComponent hostComponent, EntityUid guardian, GuardianComponent guardianComponent)
        {
            if (!guardianComponent.GuardianLoose)
            {
                DebugTools.Assert(hostComponent.GuardianContainer.Contains(guardian));
                return;
            }

            _container.Insert(guardian, hostComponent.GuardianContainer);
            DebugTools.Assert(hostComponent.GuardianContainer.Contains(guardian));
            _popupSystem.PopupEntity(Loc.GetString("guardian-entity-recall"), host);
            guardianComponent.GuardianLoose = false;
        }
    }
}