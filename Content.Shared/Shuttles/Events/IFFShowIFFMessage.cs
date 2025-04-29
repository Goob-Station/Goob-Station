// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

/// <summary>
/// Raised on a client IFF console when it wishes to show IFF.
/// </summary>
[Serializable, NetSerializable]
public sealed class IFFShowIFFMessage : BoundUserInterfaceMessage
{
    public bool Show;
}