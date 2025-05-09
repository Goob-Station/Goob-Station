// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Blob.Events;


[Serializable, NetSerializable]
public sealed class BlobAttackEvent : EntityEventArgs
{
    public readonly Vector2 Position;
    public readonly NetEntity BlobEntity;
    public readonly NetEntity AttackedEntity;

    public BlobAttackEvent(NetEntity blobEntity, NetEntity attackedEntity, Vector2 position)
    {
        Position = position;
        BlobEntity = blobEntity;
        AttackedEntity = attackedEntity;
    }
}