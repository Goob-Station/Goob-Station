// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared._EinsteinEngines.Flight.Events;

[Serializable, NetSerializable]
public sealed partial class DashDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class FlightDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed class FlightEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public bool IsFlying { get; }
    public bool IsAnimated { get; }
    public FlightEvent(NetEntity uid, bool isFlying, bool isAnimated)
    {
        Uid = uid;
        IsFlying = isFlying;
        IsAnimated = isAnimated;
    }
}