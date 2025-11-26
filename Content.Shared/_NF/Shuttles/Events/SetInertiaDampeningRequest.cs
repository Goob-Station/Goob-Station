// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Robust.Shared.Serialization;

namespace Content.Shared._NF.Shuttles.Events;

/// <summary>
/// Raised on the client when it wishes to change the inertial dampening of a ship.
/// </summary>
[Serializable, NetSerializable]
public sealed class SetInertiaDampeningRequest : BoundUserInterfaceMessage
{
    public NetEntity? ShuttleEntityUid { get; set; }
    public InertiaDampeningMode Mode { get; set; }
}

[Serializable, NetSerializable]
public enum InertiaDampeningMode : byte
{
    None = 0, // Reserved for requests - does not set the mode, only returns its state.
    Cruise,
    Dampen,
    Anchor,
}
