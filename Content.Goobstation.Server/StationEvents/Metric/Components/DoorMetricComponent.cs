// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(DoorMetricSystem))]
public sealed partial class DoorMetricComponent : Component
{
    /// <summary>
    ///   Cost of all doors emagged door
    /// </summary>
    [DataField]
    public double EmagCost = 200.0f;

    /// <summary>
    ///   Cost of all doors with no power
    /// </summary>
    [DataField]
    public double PowerCost = 100.0f;

    /// <summary>
    ///   Cost of all firedoors holding pressure
    /// </summary>
    [DataField]
    public double PressureCost = 200.0f;

    /// <summary>
    ///   Cost of all firedoors holding temperature
    /// </summary>
    [DataField]
    public double FireCost = 400.0f;
}
