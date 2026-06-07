// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared._Lavaland.Megafauna.Conditions.Targeting;

namespace Content.Shared._Lavaland.Megafauna.Conditions;

/// <summary>
/// Condition that returns true if the target is at specific range from the boss.
/// Returns false if out of range, or target is null.
/// </summary>
public sealed partial class RangeCondition : MegafaunaEntityCondition
{
    [DataField]
    public float? MinRange;

    [DataField]
    public float? MaxRange;

    public override bool EvaluateImplementation(MegafaunaCalculationBaseArgs args, EntityUid target)
    {
        var entMan = args.EntityManager;
        var transformSys = entMan.System<SharedTransformSystem>();

        var bossPos = transformSys.GetMapCoordinates(args.Entity);
        var targetPos = transformSys.GetMapCoordinates(target);

        if (bossPos.MapId != targetPos.MapId)
            return false;

        var distance = Vector2.Distance(bossPos.Position, targetPos.Position);

        return distance > (MinRange ?? -1f) && distance < (MaxRange ?? float.MaxValue);
    }
}
