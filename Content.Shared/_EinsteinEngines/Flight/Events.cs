// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
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