using Content.Shared._Lavaland.Tile.Shapes;

namespace Content.Shared._Lavaland.Megafauna.Operators;

public sealed partial class TileShapeSizeOperator : MegafaunaActionOperator
{
    [ViewVariables]
    public TileShape Shape;

    public override object GetValue(int counter)
    {
        Shape.DefaultSize += counter;
        return Shape;
    }
}
