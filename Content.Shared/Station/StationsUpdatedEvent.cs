// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Station;

[NetSerializable, Serializable]
public sealed class StationsUpdatedEvent : EntityEventArgs
{
    public readonly List<(string Name, NetEntity Entity)> Stations;

    public StationsUpdatedEvent(List<(string Name, NetEntity Entity)> stations)
    {
        Stations = stations;
    }
}
