// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.BagOfHolding
{
    public sealed class BagOfHoldingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BoHComponent, InteractUsingEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, BoHComponent component, InteractUsingEvent args)
        {
            _entityManager.TryGetComponent<BoHComponent>(args.Used, out var secondBoh);
            if (secondBoh == null)
                return;

            var newUid = Spawn("Singularity", Transform(uid).Coordinates);

            _adminLogger.Add(
                LogType.Action, LogImpact.High,
                $"{ToPrettyString(args.User):actor} created {ToPrettyString(newUid)} by putting {ToPrettyString(args.Used)} into {ToPrettyString(uid)}");

            QueueDel(args.Used);
        }
    }
}
