// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Megafauna.Operators;
using Content.Shared._Lavaland.Tile;
using Content.Shared._Lavaland.Tile.Shapes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Actions;

[MeansImplicitUse]
public sealed partial class SpawnPatternAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public TileShape Shape;

    [DataField]
    public TileShapeSizeOperator? ShapeOperator;

    [DataField]
    public EntProtoId SpawnId;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var shapeSpawner = args.EntityManager.System<TileShapeSystem>();
        var target = args.AiComponent.CurrentTarget ?? args.BossEntity;

        TileShape? modifiedShape = null;
        if (ShapeOperator != null)
        {
            ShapeOperator.Shape = Shape;
            modifiedShape = (TileShape) ShapeOperator.GetValue(Counter);
        }

        shapeSpawner.SpawnTileShape(modifiedShape ?? Shape, target, SpawnId, out _);

        return DelaySelector.Get(args);
    }
}
