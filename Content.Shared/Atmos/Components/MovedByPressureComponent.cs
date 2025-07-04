// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Atmos.Components;

// Unfortunately can't be friends yet due to magboots.
[RegisterComponent]
public sealed partial class MovedByPressureComponent : Component
{
    public const float MoveForcePushRatio = 1f;
    public const float MoveForceForcePushRatio = 1f;
    public const float ProbabilityOffset = 25f;
    public const float ProbabilityBasePercent = 10f;
    public const float ThrowForce = 100f;

    /// <summary>
    /// Accumulates time when yeeted by high pressure deltas.
    /// </summary>
    [DataField]
    public float Accumulator;

    [DataField]
    public bool Enabled { get; set; } = true;

    [DataField]
    public float PressureResistance { get; set; } = 1f;

    [DataField]
    public float MoveResist { get; set; } = 100f;

    [ViewVariables(VVAccess.ReadWrite)]
    public int LastHighPressureMovementAirCycle { get; set; } = 0;
}
