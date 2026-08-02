// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared._EinsteinEngines.Flight.Events;

[Serializable, NetSerializable]
public sealed partial class DashDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class FlightDoAfterEvent : SimpleDoAfterEvent { }

public sealed class FlightEvent : EntityEventArgs
{
    public EntityUid Uid { get; }
    public bool IsFlying { get; }
    public bool IsAnimated { get; }
    public FlightEvent(EntityUid uid, bool isFlying, bool isAnimated)
    {
        Uid = uid;
        IsFlying = isFlying;
        IsAnimated = isAnimated;
    }
}

[ByRefEvent]
public sealed class FlightAttemptEvent : CancellableEntityEventArgs {}

[Serializable, NetSerializable]
public sealed class ToggleFlightVisualsEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public bool IsFlying { get; }

    public bool IsAnimated { get; }
    public ToggleFlightVisualsEvent(NetEntity uid, bool isFlying, bool isAnimated)
    {
        Uid = uid;
        IsFlying = isFlying;
        IsAnimated = isAnimated;
    }
}