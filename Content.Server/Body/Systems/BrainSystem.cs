// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Server.Ghost.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Pointing;

// Shitmed Change
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Systems;

namespace Content.Server.Body.Systems
{
    public sealed class BrainSystem : EntitySystem
    {
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;
        [Dependency] private readonly SharedBodySystem _bodySystem = default!; // Shitmed Change
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BrainComponent, OrganAddedToBodyEvent>(HandleAddition);
        // Shitmed Change Start
            SubscribeLocalEvent<BrainComponent, OrganRemovedFromBodyEvent>(HandleRemoval);
            SubscribeLocalEvent<BrainComponent, PointAttemptEvent>(OnPointAttempt);
        }

        private void HandleRemoval(EntityUid uid, BrainComponent brain, ref OrganRemovedFromBodyEvent args)
        {
            if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.OldBody))
                return;

            brain.Active = false;
            if (!CheckOtherBrains(args.OldBody))
            {
                // Prevents revival, should kill the user within a given timespan too.
                EnsureComp<DebrainedComponent>(args.OldBody);
                HandleMind(uid, args.OldBody);
            }
        }

        private void HandleAddition(EntityUid uid, BrainComponent brain, ref OrganAddedToBodyEvent args)
        {
            if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.Body))
                return;

            if (!CheckOtherBrains(args.Body))
            {
                RemComp<DebrainedComponent>(args.Body);
                HandleMind(args.Body, uid, brain);
            }
        }


        private void HandleMind(EntityUid newEntity, EntityUid oldEntity, BrainComponent? brain = null)
        {
            if (TerminatingOrDeleted(newEntity) || TerminatingOrDeleted(oldEntity))
                return;

            EnsureComp<MindContainerComponent>(newEntity);
            EnsureComp<MindContainerComponent>(oldEntity);

            var ghostOnMove = EnsureComp<GhostOnMoveComponent>(newEntity);
            if (HasComp<BodyComponent>(newEntity))
                ghostOnMove.MustBeDead = true;

            if (!_mindSystem.TryGetMind(oldEntity, out var mindId, out var mind))
                return;

            _mindSystem.TransferTo(mindId, newEntity, mind: mind);
            if (brain != null)
                brain.Active = true;
        }

        private bool CheckOtherBrains(EntityUid entity)
        {
            var hasOtherBrains = false;
            if (TryComp<BodyComponent>(entity, out var body))
            {
                if (TryComp<BrainComponent>(entity, out var bodyBrain))
                    hasOtherBrains = true;
                else
                {
                    foreach (var (organ, _) in _bodySystem.GetBodyOrgans(entity, body))
                    {
                        if (TryComp<BrainComponent>(organ, out var brain) && brain.Active)
                        {
                            hasOtherBrains = true;
                            break;
                        }
                    }
                }
            }

            return hasOtherBrains;
        }

        // Shitmed Change End
        private void OnPointAttempt(Entity<BrainComponent> ent, ref PointAttemptEvent args)
        {
            args.Cancel();
        }
    }
}