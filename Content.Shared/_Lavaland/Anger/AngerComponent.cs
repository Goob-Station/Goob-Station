// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Anger;

/// <summary>
/// Makes megafauna stronger when it takes more damage.
/// Aggression value can be used in MegafaunaActions to control their power.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AngerComponent : Component
{
    /// <summary>
    /// Current percentage of anger. By just receiving damage goes up to 1,
    /// but can be
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float CurrentAnger = 0f;

    /// <summary>
    /// Total HP of a boss.
    /// Gets set to Dead MobThreshold when megafauna
    /// initializes, and backwards when shutting down.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 BaseTotalHp = 1;

    [DataField]
    public float AngerScalingFactor = 1.2f;

    [DataField]
    public float HealthScalingFactor = 1.15f;

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
    public float MinAnger;

    /// <summary>
    /// The maximum value of anger that always stays the same.
    /// Prevents some crazy bugs when AI kills the server by spamming with a lot of things with crazy high anger.
    /// </summary>
    [DataField]
    public float MaxAnger = 2f;
}
