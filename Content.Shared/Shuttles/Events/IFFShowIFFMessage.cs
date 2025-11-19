// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

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
