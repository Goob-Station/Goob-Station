// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Radiation.Components;

/// <summary>
///     Irradiate all objects in range.
/// </summary>
[RegisterComponent]
public sealed partial class RadiationSourceComponent : Component
{
    /// <summary>
    ///     Radiation intensity in center of the source in rads per second.
    ///     From there radiation rays will travel over distance and loose intensity
    ///     when hit radiation blocker.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("intensity")]
    public float Intensity = 1;

    /// <summary>
    ///     GOOBSTATION
    ///     Defines how fast radiation rays will loose intensity
    ///     over distance if the ray enters terminal decay. The bigger the value, faster the radiation source
    ///     will decay past the TerminalDecayDistance.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("terminalDecaySlope")]
    public float TerminalDecaySlope = 0.07f;

    /// <summary>
    ///     GOOBSTATION
    ///     Defines distance from source until a radiation ray enters terminal decay.
    ///     Increasing the value increases the distance the the ray will operate under pure hyperbolic decay.
    ///     Hyperbolic decay is horizontially asymptotic at y=0. Terminal decay is an additional
    ///     linear decrement.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("terminalDecayDistance")]
    public float TerminalDecayDistance = 15;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled = true;
}
