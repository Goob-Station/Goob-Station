﻿using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Creates a filled box, but also with a chance of a tile to be missing, making it have random cavities.
/// </summary>
public sealed partial class BoxRandomTileShape : TileShape
{
    /// <summary>
    /// The chance for a tile to be filled in this random box.
    /// Always overrides RemoveAmount
    /// </summary>
    [DataField]
    public float? FilledChance;

    /// <summary>
    /// How many tiles we should exclude from a filled box.
    /// </summary>
    [DataField]
    public int? RemoveAmount;

    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        if (FilledChance != null)
            return TileHelperMethods.MakeBoxChanceRandom(Offset, Size, rand, FilledChance.Value);
        if (RemoveAmount != null)
            return TileHelperMethods.MakeBoxCountRandom(Offset, Size, rand, RemoveAmount.Value);

        return TileHelperMethods.MakeBoxFilled(Offset, Size);
    }
}
