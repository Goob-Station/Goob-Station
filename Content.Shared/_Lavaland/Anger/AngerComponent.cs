// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Lavaland.Anger;

/// <summary>
/// Makes megafauna stronger when it takes more damage.
/// Aggression value can be used in MegafaunaActions to control their power.
/// </summary>
[RegisterComponent]
public sealed partial class AngerComponent : Component
{
    [ViewVariables]
    public float CurrentAnger = 1f;

    [DataField]
    public float AngerScalingFactor = 1.2f;

    [DataField]
    public float HealthScalingFactor = 1.15f;

    /// <summary>
    /// Total HP of a boss.
    /// Gets set to Dead MobThreshold when megafauna
    /// initializes, and backwards when shutting down.
    /// </summary>
    [ViewVariables]
    public FixedPoint2 BaseTotalHp = 1;

    /// <summary>
    /// If specified, anger will be scaled not just according to scaled BaseTotalHp, but
    /// instead of BaseTotalHp this value will be used.
    /// Great to use when you want to balance anger better.
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
}
