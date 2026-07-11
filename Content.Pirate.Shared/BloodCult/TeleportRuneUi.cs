// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.BloodCult;

[Serializable, NetSerializable]
public enum TeleportRuneNameUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class TeleportRuneNameSelectedMessage(string name) : BoundUserInterfaceMessage
{
    public string Name { get; } = name;
}
