// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

/// <summary>
/// Raised on the client when it wishes to not have 2 docking ports docked.
/// </summary>
[Serializable, NetSerializable]
public sealed class UndockRequestMessage : BoundUserInterfaceMessage
{
    public NetEntity DockEntity;
}