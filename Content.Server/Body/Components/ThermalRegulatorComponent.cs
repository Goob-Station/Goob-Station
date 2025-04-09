// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Body.Components;

[RegisterComponent]
[Access(typeof(ThermalRegulatorSystem))]
public sealed partial class ThermalRegulatorComponent : Component
{
    /// <summary>
    /// The next time that the body will regulate its heat.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate;

    /// <summary>
    /// The interval at which thermal regulation is processed.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Heat generated due to metabolism. It's generated via metabolism
    /// </summary>
    [DataField]
    public float MetabolismHeat;

    /// <summary>
    /// Heat output via radiation.
    /// </summary>
    [DataField]
    public float RadiatedHeat;

    /// <summary>
    /// Maximum heat regulated via sweat
    /// </summary>
    [DataField]
    public float SweatHeatRegulation;

    /// <summary>
    /// Maximum heat regulated via shivering
    /// </summary>
    [DataField]
    public float ShiveringHeatRegulation;

    /// <summary>
    /// Amount of heat regulation that represents thermal regulation processes not
    /// explicitly coded.
    /// </summary>
    [DataField]
    public float ImplicitHeatRegulation;

    /// <summary>
    /// Normal body temperature
    /// </summary>
    [DataField]
    public float NormalBodyTemperature;

    /// <summary>
    /// Deviation from normal temperature for body to start thermal regulation
    /// </summary>
    [DataField]
    public float ThermalRegulationTemperatureThreshold;
}