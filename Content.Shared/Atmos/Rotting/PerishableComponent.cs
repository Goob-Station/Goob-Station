// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Sir Winters <7543955+Owai-Seek@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Atmos.Rotting;

/// <summary>
/// This makes mobs eventually start rotting when they die.
/// It may be expanded to food at some point, but it's just for mobs right now.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedRottingSystem))]
public sealed partial class PerishableComponent : Component
{
    /// <summary>
    /// How long it takes after death to start rotting.
    /// </summary>
    [DataField]
    public TimeSpan RotAfter = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How much rotting has occured
    /// </summary>
    [DataField]
    public TimeSpan RotAccumulator = TimeSpan.Zero;

    /// <summary>
    /// Gasses are released, this is when the next gas release update will be.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan RotNextUpdate = TimeSpan.Zero;

    /// <summary>
    /// How often the rotting ticks.
    /// Feel free to tweak this if there are perf concerns.
    /// </summary>
    [DataField]
    public TimeSpan PerishUpdateRate = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How many moles of gas released per second, per unit of mass.
    /// </summary>
    [DataField]
    public float MolsPerSecondPerUnitMass = 0.0025f;

    [DataField, AutoNetworkedField]
    public int Stage;

    /// <summary>
    /// If true, rot will always progress.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ForceRotProgression;
}


[ByRefEvent]
public record struct IsRottingEvent(bool Handled = false);