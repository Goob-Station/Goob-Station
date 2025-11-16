// SPDX-FileCopyrightText: 2025 ThanosDeGraf <richardgirgindontstop@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Body.Components;

public sealed partial class RespiratorComponent : Component
{
    /// <summary>
    /// Goob: Multiplier on saturation passively lost.
    /// Higher values require more air, lower require less.
    /// Multiplicative with a lung's <see cref="LungComponent.SaturationLoss"/>
    /// </summary>
    [DataField]
    public float SaturationLoss = 1f;
}
