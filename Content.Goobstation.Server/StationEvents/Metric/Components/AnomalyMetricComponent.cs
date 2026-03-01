// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(AnomalyMetric))]
public sealed partial class AnomalyMetricComponent : Component
{
    /// <summary>
    ///   Cost of a growing anomaly
    /// </summary>
    [DataField]
    public float GrowingCost = 40.0f;

    /// <summary>
    ///   Cost of a dangerous anomaly
    /// </summary>
    [DataField]
    public float SeverityCost = 20.0f;

    /// <summary>
    ///   Cost of any anomaly
    /// </summary>
    [DataField("dangerCost")]
    public float BaseCost = 10.0f;
}
