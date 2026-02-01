#region DOWNSTREAM-TPirates: ghost follow menu update
using System.Linq;
using System.Numerics;
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
        TryGetStyleProperty(StylePropertySeparation, out int separation) ? separation : SeparationOverride ?? DefaultSeparation;

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var sep = ActualSeparation;
        var visible = Children.Where(c => c.Visible).ToList();
        if (visible.Count == 0)
            return Vector2.Zero;

        foreach (var child in visible)
            child.Measure(new Vector2(availableSize.X, float.PositiveInfinity));

        float x = 0, y = 0, rowHeight = 0;
        var firstInRow = true;
        foreach (var child in visible)
        {
            var w = child.DesiredSize.X;
            var h = child.DesiredSize.Y;
            if (!firstInRow && x + sep + w > availableSize.X)
            {
                y += rowHeight + sep;
                x = 0;
                rowHeight = 0;
                firstInRow = true;
            }
            if (!firstInRow)
                x += sep;
            firstInRow = false;
            x += w;
            rowHeight = Math.Max(rowHeight, h);
        }
        y += rowHeight;
        return new Vector2(availableSize.X, y);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var sep = ActualSeparation;
        var visible = Children.Where(c => c.Visible).ToList();
        if (visible.Count == 0)
            return finalSize;

        float x = 0, y = 0, rowHeight = 0;
        var firstInRow = true;
        foreach (var child in visible)
        {
            var w = child.DesiredSize.X;
            var h = child.DesiredSize.Y;
            if (!firstInRow && x + sep + w > finalSize.X)
            {
                y += rowHeight + sep;
                x = 0;
                rowHeight = 0;
                firstInRow = true;
            }
            if (!firstInRow)
                x += sep;
            firstInRow = false;
            child.Arrange(UIBox2.FromDimensions(x, y, w, h));
            x += w;
            rowHeight = Math.Max(rowHeight, h);
        }
        return finalSize;
    }
}
#endregion
