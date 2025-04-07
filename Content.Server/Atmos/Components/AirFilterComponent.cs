// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
﻿using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components;

/// <summary>
/// This is basically a reverse scrubber but using <see cref="GetFilterAirEvent"/>.
/// </summary>
[RegisterComponent, Access(typeof(AirFilterSystem))]
public sealed partial class AirFilterComponent : Component
{
    /// <summary>
    /// Gases that will be filtered out of internal air
    /// </summary>
    [DataField(required: true)]
    public HashSet<Gas> Gases = new();

    /// <summary>
    /// Gases that will be filtered out of internal air to maintain oxygen ratio.
    /// When oxygen is below <see cref="TargetOxygen"/>, these gases will be filtered instead of <see cref="Gases"/>.
    /// </summary>
    [DataField(required: true)]
    public HashSet<Gas> OverflowGases = new();

    /// <summary>
    /// Minimum oxygen fraction before it will start removing <see cref="OverflowGases"/>.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float TargetOxygen = 0.21f;

    /// <summary>
    /// Gas to consider oxygen for <see cref="TargetOxygen"/> and <see cref="OverflowGases"/> logic.
    /// </summary>
    /// <remarks>
    /// For slime you might want to change this to be nitrogen, and overflowgases to remove oxygen.
    /// However theres still no real danger since standard atmos is mostly nitrogen so nitrogen tends to 100% anyway.
    /// </remarks>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Gas Oxygen = Gas.Oxygen;

    /// <summary>
    /// Fraction of target volume to transfer every second.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float TransferRate = 0.1f;
}