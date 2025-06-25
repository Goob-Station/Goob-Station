// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server._Lavaland.Megafauna.Components;

/// <summary>
/// Makes megafauna stronger when it takes more damage,
/// modifying cooldowns or attacks themselves.
/// This is the simplest way to add behavior to megafauna.
/// </summary>
[RegisterComponent]
public sealed partial class AggressiveMegafaunaAiComponent : Component
{
    /// <summary>
    /// List of all available megafauna attacks to execute.
    /// Filled on component startup from the other component.
    /// If there's <see cref="PhasesMegafaunaAiComponent"/>, this value is ignored.
    /// </summary>
    [DataField]
    public List<MegafaunaAction> ActionsData = new();

    [ViewVariables]
    public float CurrentAnger = 1f;

    [DataField]
    public float AngerScalingFactor = 1.2f;

    [DataField]
    public float HealthScalingFactor = 1.15f;

    /// <summary>
    /// If specified, aggression will be scaled not just according to scaled BaseTotalHp, but
    /// instead of BaseTotalHp this value will be used.
    /// Great to use when you want to balance aggression better.
    /// </summary>
    [DataField]
    public FixedPoint2? HpAgressionLimit;

    [DataField]
    public float AdjustAngerOnAttack = 0.1f;

    [DataField]
    public float MinAnger = 1f;

    /// <summary>
    /// Softcap for anger that will be reached when megafauna will take the limit damage.
    /// </summary>
    [DataField]
    public float MaxAnger = 3f;

    /// <summary>
    /// The maximum value of anger that always stays the same.
    /// Prevents some crazy bugs when AI kills the server by spamming with a lot of things with crazy high anger.
    /// </summary>
    [DataField]
    public float MaxAngerHardCap = 6f;

    [ViewVariables]
    public float UpdateAccumulator = 1f;

    [DataField]
    public float UpdatePeriod = 1f;
}
