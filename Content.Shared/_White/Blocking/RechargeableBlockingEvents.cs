// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._White.Blocking;

[Serializable, NetSerializable]
public sealed class ForceTurnOffToggleActiveSound(NetEntity item) : EntityEventArgs
{
    public NetEntity ToggleItem = item;
}
