#region DOWNSTREAM-TPirates: ghost follow menu update
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface.Controls;

/// <summary>
/// Lays out children in rows: left-to-right, wrapping to the next row when the next child
/// would exceed the available width. Each child gets its desired size (no fixed grid cells).
/// Similar to CSS flex-wrap.
/// </summary>
[Virtual]
public class WrapContainer : Container
{
    public const string StylePropertySeparation = "separation";
    private const int DefaultSeparation = 4;

    public int? SeparationOverride { get; set; }

    private int ActualSeparation =>
        SeparationOverride ?? (TryGetStyleProperty(StylePropertySeparation, out int separation) ? separation : DefaultSeparation);

    /// <summary>Computes row-wrap layout over visible children. Calls onArrange for each (x,y,w,h) when non-null. Returns (total height, true if any child).</summary>
    private (float TotalHeight, bool HadChildren) ComputeLayout(
        float maxWidth,
        int sep,
        IEnumerable<Control> visible,
        ArrangeChild? onArrange)
    {
        float x = 0, y = 0, rowHeight = 0;
        var firstInRow = true;
        var hadChildren = false;
        foreach (var child in visible)
        {
            hadChildren = true;
            var w = child.DesiredSize.X;
            var h = child.DesiredSize.Y;
            if (!firstInRow && x + sep + w > maxWidth)
            {
                y += rowHeight + sep;
                x = 0;
                rowHeight = 0;
                firstInRow = true;
            }
            if (!firstInRow)
                x += sep;
            firstInRow = false;
            onArrange?.Invoke(child, x, y, w, h);
            x += w;
            rowHeight = Math.Max(rowHeight, h);
        }
        return (y + rowHeight, hadChildren);
    }

    private delegate void ArrangeChild(Control child, float x, float y, float w, float h);

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var sep = ActualSeparation;
        var visible = Children.Where(c => c.Visible).ToList();
        foreach (var child in visible)
            child.Measure(new Vector2(availableSize.X, float.PositiveInfinity));
        float maxRight = 0f;
        var (totalHeight, hadChildren) = ComputeLayout(availableSize.X, sep, visible,
            (child, x, y, w, h) => maxRight = Math.Max(maxRight, x + w));
        if (!hadChildren)
            return Vector2.Zero;
        var desiredWidth = float.IsPositiveInfinity(availableSize.X) ? maxRight : availableSize.X;
        return new Vector2(desiredWidth, totalHeight);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var sep = ActualSeparation;
        var visible = Children.Where(c => c.Visible);
        ComputeLayout(finalSize.X, sep, visible, (child, x, y, w, h) =>
            child.Arrange(UIBox2.FromDimensions(x, y, w, h)));
        return finalSize;
    }
}
#endregion
