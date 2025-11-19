// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server._Funkystation.Objectives.Systems;

namespace Content.Server._Funkystation.Objectives.Components;

/// <summary>
/// Component that scales the required number of borgs based on player count for Malf AI objectives.
/// </summary>
[RegisterComponent, Access(typeof(MalfAiScaleBorgsObjectiveSystem))]
public sealed partial class MalfAiScaleBorgsObjectiveComponent : Component
{
    /// <summary>
    /// Target number of borgs to control, calculated based on player count.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int Target;

    /// <summary>
    /// Players per borg ratio. For every X players, require 1 borg.
    /// </summary>
    [DataField]
    public int PlayersPerBorg = 10;

    /// <summary>
    /// Minimum number of borgs required regardless of player count.
    /// </summary>
    [DataField]
    public int MinBorgs = 2;

    /// <summary>
    /// Maximum number of borgs required regardless of player count.
    /// </summary>
    [DataField]
    public int MaxBorgs = 5;
}
