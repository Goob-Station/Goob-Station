// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Shuttles.Systems;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// Added to a station that is available for arrivals shuttles.
/// </summary>
[RegisterComponent, Access(typeof(ArrivalsSystem))]
public sealed partial class StationArrivalsComponent : Component
{
    [DataField("shuttle")]
    public EntityUid Shuttle;

    [DataField("shuttlePath")] public ResPath ShuttlePath = new("/Maps/Shuttles/arrivals.yml");

    /// <summary>
    /// Per-station override for whether the terminal is a planet (adds biome).
    /// Null falls back to the CVar.
    /// </summary>
    [DataField("isPlanet")]
    public bool? IsPlanet;

    /// <summary>
    /// Per-station override for whether the shuttle can return to the terminal.
    /// Null falls back to the CVar.
    /// </summary>
    [DataField("allowReturns")]
    public bool? AllowReturns;

    /// <summary>
    /// When true, the arrivals shuttle will return to the terminal and stay there
    /// while the emergency shuttle is present on the station map.
    /// </summary>
    [DataField("pauseDuringEvac")]
    public bool PauseDuringEvac;

    /// <summary>
    /// Set at runtime when evac is active. Cleared when evac is recalled.
    /// </summary>
    [ViewVariables]
    public bool EvacActive;

    /// <summary>
    /// The terminal grid entity for this station. Set at runtime for stations with overrides.
    /// </summary>
    [ViewVariables]
    public EntityUid? TerminalGrid;
}
