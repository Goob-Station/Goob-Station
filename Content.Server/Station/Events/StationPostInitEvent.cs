// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Station.Components;

namespace Content.Server.Station.Events;

/// <summary>
/// Raised directed on a station after it has been initialized, as well as broadcast.
/// This gets raised after the entity has been map-initialized, and the station's centcomm map/entity (if any) has been
/// set up.
/// </summary>
[ByRefEvent]
public readonly record struct StationPostInitEvent(Entity<StationDataComponent> Station);