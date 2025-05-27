// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Content.Server.Body.Systems;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.SpawnOnInteract
{
    public sealed class SpawnOnInteractSystem : EntitySystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IPrototypeManager _protoManager = default!;
        [Dependency] private readonly BodySystem _bodySystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SpawnOnInteractComponent, InteractUsingEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, SpawnOnInteractComponent interactedComp, InteractUsingEvent args)
        {
            if (!EntityManager.TryGetComponent<SpawnOnInteractComponent>(args.Used, out var interactingComp))
                return;

            if (!_protoManager.HasIndex<EntityPrototype>(interactedComp.ToSpawn))
                return;

            var newUid = Spawn(interactedComp.ToSpawn, Transform(uid).Coordinates);

            _adminLogger.Add(
                LogType.Action, LogImpact.High,
                $"{ToPrettyString(args.User):actor} created {ToPrettyString(newUid)} by intracting {ToPrettyString(args.Used)} with {ToPrettyString(uid)}");

            if (interactedComp.DeleteInteractedEnt == true)
                QueueDel(uid);
            if (interactingComp.DeleteInteractingEnt == true)
                QueueDel(args.Used);
            if (interactingComp.GibUser == true || interactedComp.GibUser == true)
                _bodySystem.GibBody(args.User);
        }
    }
}
