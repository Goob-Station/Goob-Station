// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Objective has a target number of something.
/// When the objective is assigned it randomly picks this target from a minimum to a maximum.
/// </summary>
[RegisterComponent, Access(typeof(NumberObjectiveSystem))]
public sealed partial class NumberObjectiveComponent : Component
{
    /// <summary>
    /// Number to use in the objective condition.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int Target;

    /// <summary>
    /// Minimum number for target to roll.
    /// </summary>
    [DataField(required: true)]
    public int Min;

    /// <summary>
    /// Maximum number for target to roll.
    /// </summary>
    [DataField(required: true)]
    public int Max;

    /// <summary>
    /// Optional title locale id, passed "count" with <see cref="Target"/>.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string? Title;

    /// <summary>
    /// Optional description locale id, passed "count" with <see cref="Target"/>.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string? Description;
}