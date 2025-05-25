// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

/// <summary>
/// Raised on the client when it's viewing a particular docking port to try and dock it.
/// </summary>
[Serializable, NetSerializable]
public sealed class DockRequestMessage : BoundUserInterfaceMessage
{
    public NetEntity DockEntity;

    public NetEntity TargetDockEntity;
}