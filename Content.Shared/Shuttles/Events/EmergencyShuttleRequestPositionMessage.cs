// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

/// <summary>
/// Raised on the client to request the expected position of the emergency shuttle for debugging.
/// </summary>
[Serializable, NetSerializable]
public sealed class EmergencyShuttleRequestPositionMessage : EntityEventArgs
{

}
