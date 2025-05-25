// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Shared._Shitmed.Spawners.EntitySystems;

public sealed class SpawnerSpawnedEvent : EntityEventArgs
{
    public EntityUid Entity { get; }

    public bool IsFriendly { get; }
    public SpawnerSpawnedEvent(EntityUid entity, bool isFriendly)
    {
        Entity = entity;
        IsFriendly = isFriendly;
    }
}