// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Hierophant.Components;

/// <summary>
/// Shitcode so we can just not use Timer.Spawn.
/// TODO: think about better solutions regarding this thing.
/// </summary>
[RegisterComponent]
public sealed partial class HierophantAttackSpawnerComponent : Component
{
    [ViewVariables]
    public HierophantRepeatAttackType[] AttacksOrder;

    [ViewVariables]
    public int FinalCounter = 5;

    [ViewVariables]
    public int Counter;

    [ViewVariables]
    public float Accumulator;

    [DataField]
    public float AttackDelay = 0.6f;

    [DataField]
    public EntProtoId TileId;
}

public enum HierophantRepeatAttackType
{
    Invalid,
    CrossRook,
    CrossBishop,
    CrossQueen,
    BoxHollow,
    BoxFilled,
}
