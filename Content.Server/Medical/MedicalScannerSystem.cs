// SPDX-FileCopyrightText: 2018 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2019 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 SoulSloth <67545203+SoulSloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Fishfish458 <47410468+Fishfish458@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 fishfish458 <fishfish458>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Cloning;
using Content.Server.Medical.Components;
using Content.Shared.Destructible;
using Content.Shared.ActionBlocker;
using Content.Shared.DragDrop;
using Content.Shared.Movement.Events;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Content.Server.Cloning.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking.Events;
using Content.Server.Power.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Climbing.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.Containers;
using static Content.Shared.MedicalScanner.SharedMedicalScannerComponent; // Hmm...

namespace Content.Server.Medical
{
    public sealed class MedicalScannerSystem : EntitySystem
    {
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
        [Dependency] private readonly ActionBlockerSystem _blocker = default!;
        [Dependency] private readonly ClimbSystem _climbSystem = default!;
        [Dependency] private readonly CloningConsoleSystem _cloningConsoleSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

        private const float UpdateRate = 1f;
        private float _updateDif;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MedicalScannerComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<MedicalScannerComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
            SubscribeLocalEvent<MedicalScannerComponent, GetVerbsEvent<InteractionVerb>>(AddInsertOtherVerb);
            SubscribeLocalEvent<MedicalScannerComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
            SubscribeLocalEvent<MedicalScannerComponent, DestructionEventArgs>(OnDestroyed);
            SubscribeLocalEvent<MedicalScannerComponent, DragDropTargetEvent>(OnDragDropOn);
            SubscribeLocalEvent<MedicalScannerComponent, PortDisconnectedEvent>(OnPortDisconnected);
            SubscribeLocalEvent<MedicalScannerComponent, AnchorStateChangedEvent>(OnAnchorChanged);
            SubscribeLocalEvent<MedicalScannerComponent, CanDropTargetEvent>(OnCanDragDropOn);
        }

        private void OnCanDragDropOn(EntityUid uid, MedicalScannerComponent component, ref CanDropTargetEvent args)
        {
            args.Handled = true;
            args.CanDrop |= CanScannerInsert(uid, args.Dragged, component);
        }

        public bool CanScannerInsert(EntityUid uid, EntityUid target, MedicalScannerComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return false;

            return HasComp<BodyComponent>(target);
        }

        private void OnComponentInit(EntityUid uid, MedicalScannerComponent scannerComponent, ComponentInit args)
        {
            base.Initialize();
            scannerComponent.BodyContainer = _containerSystem.EnsureContainer<ContainerSlot>(uid, $"scanner-bodyContainer");
            _signalSystem.EnsureSinkPorts(uid, MedicalScannerComponent.ScannerPort);
        }

        private void OnRelayMovement(EntityUid uid, MedicalScannerComponent scannerComponent, ref ContainerRelayMovementEntityEvent args)
        {
            if (!_blocker.CanInteract(args.Entity, uid))
                return;

            EjectBody(uid, scannerComponent);
        }

        private void AddInsertOtherVerb(EntityUid uid, MedicalScannerComponent component, GetVerbsEvent<InteractionVerb> args)
        {
            if (args.Using == null ||
                !args.CanAccess ||
                !args.CanInteract ||
                IsOccupied(component) ||
                !CanScannerInsert(uid, args.Using.Value, component))
                return;

            var name = "Unknown";
            if (TryComp(args.Using.Value, out MetaDataComponent? metadata))
                name = metadata.EntityName;

            InteractionVerb verb = new()
            {
                Act = () => InsertBody(uid, args.Target, component),
                Category = VerbCategory.Insert,
                Text = name
            };
            args.Verbs.Add(verb);
        }

        private void AddAlternativeVerbs(EntityUid uid, MedicalScannerComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract || !args.CanComplexInteract) // Goobstation - complex check
                return;

            // Eject verb
            if (IsOccupied(component))
            {
                AlternativeVerb verb = new()
                {
                    Act = () => EjectBody(uid, component),
                    Category = VerbCategory.Eject,
                    Text = Loc.GetString("medical-scanner-verb-noun-occupant"),
                    Priority = 1 // Promote to top to make ejecting the ALT-click action
                };
                args.Verbs.Add(verb);
            }

            // Self-insert verb
            if (!IsOccupied(component) &&
                CanScannerInsert(uid, args.User, component) &&
                _blocker.CanMove(args.User))
            {
                AlternativeVerb verb = new()
                {
                    Act = () => InsertBody(uid, args.User, component),
                    Text = Loc.GetString("medical-scanner-verb-enter")
                };
                args.Verbs.Add(verb);
            }
        }

        private void OnDestroyed(EntityUid uid, MedicalScannerComponent scannerComponent, DestructionEventArgs args)
        {
            EjectBody(uid, scannerComponent);
        }

        private void OnDragDropOn(EntityUid uid, MedicalScannerComponent scannerComponent, ref DragDropTargetEvent args)
        {
            InsertBody(uid, args.Dragged, scannerComponent);
        }

        private void OnPortDisconnected(EntityUid uid, MedicalScannerComponent component, PortDisconnectedEvent args)
        {
            component.ConnectedConsole = null;
        }

        private void OnAnchorChanged(EntityUid uid, MedicalScannerComponent component, ref AnchorStateChangedEvent args)
        {
            if (component.ConnectedConsole == null || !TryComp<CloningConsoleComponent>(component.ConnectedConsole, out var console))
                return;

            if (args.Anchored)
            {
                _cloningConsoleSystem.RecheckConnections(component.ConnectedConsole.Value, console.CloningPod, uid, console);
                return;
            }
            _cloningConsoleSystem.UpdateUserInterface(component.ConnectedConsole.Value, console);
        }
        private MedicalScannerStatus GetStatus(EntityUid uid, MedicalScannerComponent scannerComponent)
        {
            if (this.IsPowered(uid, EntityManager))
            {
                var body = scannerComponent.BodyContainer.ContainedEntity;
                if (body == null)
                    return MedicalScannerStatus.Open;

                if (!TryComp<MobStateComponent>(body.Value, out var state))
                {   // Is not alive or dead or critical
                    return MedicalScannerStatus.Yellow;
                }

                return GetStatusFromDamageState(body.Value, state);
            }
            return MedicalScannerStatus.Off;
        }

        public static bool IsOccupied(MedicalScannerComponent scannerComponent)
        {
            return scannerComponent.BodyContainer.ContainedEntity != null;
        }

        private MedicalScannerStatus GetStatusFromDamageState(EntityUid uid, MobStateComponent state)
        {
            if (_mobStateSystem.IsAlive(uid, state))
                return MedicalScannerStatus.Green;

            if (_mobStateSystem.IsCritical(uid, state))
                return MedicalScannerStatus.Red;

            if (_mobStateSystem.IsDead(uid, state))
                return MedicalScannerStatus.Death;

            return MedicalScannerStatus.Yellow;
        }

        private void UpdateAppearance(EntityUid uid, MedicalScannerComponent scannerComponent)
        {
            if (TryComp<AppearanceComponent>(uid, out var appearance))
            {
                _appearance.SetData(uid, MedicalScannerVisuals.Status, GetStatus(uid, scannerComponent), appearance);
            }
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            _updateDif += frameTime;
            if (_updateDif < UpdateRate)
                return;

            _updateDif -= UpdateRate;

            var query = EntityQueryEnumerator<MedicalScannerComponent>();
            while (query.MoveNext(out var uid, out var scanner))
            {
                UpdateAppearance(uid, scanner);
            }
        }

        public void InsertBody(EntityUid uid, EntityUid to_insert, MedicalScannerComponent? scannerComponent)
        {
            if (!Resolve(uid, ref scannerComponent))
                return;

            if (scannerComponent.BodyContainer.ContainedEntity != null)
                return;

            if (!HasComp<BodyComponent>(to_insert))
                return;

            _containerSystem.Insert(to_insert, scannerComponent.BodyContainer);
            UpdateAppearance(uid, scannerComponent);
        }

        public void EjectBody(EntityUid uid, MedicalScannerComponent? scannerComponent)
        {
            if (!Resolve(uid, ref scannerComponent))
                return;

            if (scannerComponent.BodyContainer.ContainedEntity is not { Valid: true } contained)
                return;

            _containerSystem.Remove(contained, scannerComponent.BodyContainer);
            _climbSystem.ForciblySetClimbing(contained, uid);
            UpdateAppearance(uid, scannerComponent);
        }
    }
}
