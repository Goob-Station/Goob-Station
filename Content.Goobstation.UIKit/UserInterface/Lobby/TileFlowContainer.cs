using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class TileFlowContainer : Container
{
    private float _arrangedWidth = float.NaN;
    private float _measuredWidth = float.NaN;

    public float Separation { get; set; } = 6;
    public float CrossSeparation { get; set; } = 6;

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var width = availableSize.X;
        if (!float.IsNaN(_arrangedWidth) && _arrangedWidth < width)
            width = _arrangedWidth;

        _measuredWidth = width;

        foreach (var child in Children)
        {
            child.Measure(new Vector2(width, float.PositiveInfinity));
        }

        return Flow(width, false);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        if (MathF.Abs(finalSize.X - _arrangedWidth) > 0.5f || float.IsNaN(_arrangedWidth))
        {
            _arrangedWidth = finalSize.X;
            if (MathF.Abs(finalSize.X - _measuredWidth) > 0.5f)
                InvalidateMeasure();
        }

        foreach (var child in Children)
        {
            if (child.Visible && !child.IsMeasureValid)
                child.Measure(new Vector2(finalSize.X, float.PositiveInfinity));
        }

        Flow(finalSize.X, true);
        return finalSize;
    }

    private Vector2 Flow(float width, bool arrange)
    {
        var x = 0f;
        var y = 0f;
        var rowHeight = 0f;
        var widest = 0f;

        foreach (var child in Children)
        {
            if (!child.Visible)
                continue;

            var size = child.DesiredSize;
            if (x > 0 && x + size.X > width)
            {
                y += rowHeight + CrossSeparation;
                x = 0;
                rowHeight = 0;
            }

            if (arrange)
                child.Arrange(UIBox2.FromDimensions(new Vector2(x, y), size));

            x += size.X + Separation;
            rowHeight = MathF.Max(rowHeight, size.Y);
            widest = MathF.Max(widest, x - Separation);
        }

        return new Vector2(MathF.Min(widest, width), y + rowHeight);
    }
}
