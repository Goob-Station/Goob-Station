// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

/// <summary>
///   Some entities (such as dragons) are just more dangerous
/// </summary>
[RegisterComponent, Access(typeof(CombatMetricSystem))]
public sealed partial class CombatPowerComponent : Component
{
    /// <summary>
    ///   Threat, expressed as a multiplier (1x is similar to a single player)
    /// </summary>
    [DataField("factor")]
    public double Threat = 1.0f;
}