using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Paint;

[Serializable, NetSerializable]
public enum PaintUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class PaintStrokeMessage(
    int sequence,
    Color color,
    int brushSize,
    PaintBrushStyle style,
    PaintShape shape,
    bool erase,
    int strokeId,
    Vector2i[] points)
    : BoundUserInterfaceMessage
{
    public readonly int Sequence = sequence;
    public readonly int StrokeId = strokeId;
    public readonly Color Color = color;
    public readonly int BrushSize = brushSize;
    public readonly PaintBrushStyle Style = style;
    public readonly PaintShape Shape = shape;
    public readonly bool Erase = erase;
    public readonly Vector2i[] Points = points;
}

[Serializable, NetSerializable]
public sealed class PaintStrokeAckMessage(int sequence, GameTick tick) : BoundUserInterfaceMessage
{
    public readonly int Sequence = sequence;
    public readonly GameTick Tick = tick;
}

[Serializable, NetSerializable]
public sealed class PaintUndoMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PaintFinishMessage(string title) : BoundUserInterfaceMessage
{
    public const int MaxTitleLength = 32;

    public readonly string Title = title;
}
