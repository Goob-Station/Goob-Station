// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Tile;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Actions;

[MeansImplicitUse]
public sealed partial class SpawnPatternAction : MegafaunaAction
{
    [DataField(required: true)]
    public ProtoId<TileShapePrototype> Shape;

    [DataField]
    public EntProtoId SpawnId = "LavalandHierophantSquare";

    [DataField]
    public float Delay = 0.7f;

    public override float Invoke(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var shapeSpawner = entMan.System<TileShapeSpawnerSystem>();

        var target = args.AiComponent.CurrentTarget ?? uid;
        shapeSpawner.SpawnTileShapeAtTarget(target, SpawnId, Shape);
        return Delay;
    }
}
