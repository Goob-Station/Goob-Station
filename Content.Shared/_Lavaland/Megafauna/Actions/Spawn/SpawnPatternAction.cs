// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
    public EntProtoId SpawnId;

    /// <summary>
    /// Is specified and this shape is a sequence, will add this amount to the size
    /// of the shape by this amount when Counter goes up.
    /// </summary>
    [DataField]
    public int? SequenceCounter;

    /// <summary>
    /// Is specified and this shape is a sequence, will offset center
    /// of the shape by this vector when Counter goes up.
    /// </summary>
    [DataField]
    public Vector2i? SequenceOffset;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var shapeSpawner = args.EntityManager.System<TileShapeSystem>();
        var target = args.AiComponent.CurrentTarget ?? args.BossEntity;

        TileShape? modifiedShape = null;
        if (IsSequence)
        {
            modifiedShape = Shape;
            modifiedShape.Size += Counter + SequenceCounter ?? 0;
            modifiedShape.Offset += SequenceOffset * Counter ?? Vector2i.Zero;
        }

        shapeSpawner.SpawnTileShape(modifiedShape ?? Shape, target, SpawnId, out _);

        return DelaySelector.Get(args);
    }
}
