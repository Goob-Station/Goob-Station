// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage.Components;
using Content.Shared.Storage.Events;

namespace Content.Shared.Storage.EntitySystems;

public sealed class StoreAfterFailedInteractSystem : EntitySystem
{
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StoreAfterFailedInteractComponent, StorageInsertFailedEvent>(OnStorageInsertFailed);
    }

    private void OnStorageInsertFailed(Entity<StoreAfterFailedInteractComponent> ent, ref StorageInsertFailedEvent args)
    {
        _storage.PlayerInsertHeldEntity(args.Storage, args.Player);
    }
}