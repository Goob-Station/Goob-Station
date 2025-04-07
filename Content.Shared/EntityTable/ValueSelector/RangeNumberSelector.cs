// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.EntityTable.ValueSelector;

/// <summary>
/// Gives a value between the two numbers specified, inclusive.
/// </summary>
public sealed partial class RangeNumberSelector : NumberSelector
{
    [DataField]
    public Vector2 Range = new(1, 1);

    public override float Get(System.Random rand, IEntityManager entMan, IPrototypeManager proto)
    {
        return rand.NextFloat(Range.X, Range.Y + 1);
    }
}