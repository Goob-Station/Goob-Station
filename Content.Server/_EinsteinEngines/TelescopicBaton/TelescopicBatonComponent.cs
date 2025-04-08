// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._EinsteinEngines.TelescopicBaton;

[RegisterComponent]
public sealed partial class TelescopicBatonComponent : Component
{
    [DataField]
    public bool CanDropItems; // Goob edit

    [DataField]
    public bool AlwaysDropItems; // Goobstation

    /// <summary>
    ///     The amount of time during which the baton will be able to knockdown someone after activating it.
    /// </summary>
    [DataField]
    public TimeSpan AttackTimeframe = TimeSpan.FromSeconds(1.8f); // Goob edit

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan TimeframeAccumulator = TimeSpan.FromSeconds(0);
}