using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._Pirate.UserInterface.Controls;

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

    /// <summary>Shared last-good width for rows rebuilt before their parents arrange.</summary>
    public WidthCache? SharedWidth { get; set; }

    public sealed class WidthCache
    {
        public float Value;
    }

    private float _lastArrangeWidth;

    private int ActualSeparation =>
        SeparationOverride ?? (TryGetStyleProperty(StylePropertySeparation, out int separation) ? separation : DefaultSeparation);

    private delegate void ArrangeChild(Control child, float x, float y, float w, float h);

    private static float ComputeLayout(
        float maxWidth,
        int sep,
        List<Control> visible,
        ArrangeChild? onArrange)
    {
        float x = 0, y = 0, rowHeight = 0;
        var firstInRow = true;
        foreach (var child in visible)
        {
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
        return y + rowHeight;
    }

    /// <summary>
    /// Picks the width to wrap at when computing desired height.
    ///
    /// Why not just <c>availableSize.X</c>? Because <see cref="BoxContainer"/>
    /// and other stretch-ratio containers pass the full available width during
    /// measure and only divide it among children during arrange. Trusting
    /// availableSize.X here over-estimates the number of columns we'll get,
    /// under-reports our desired height, and leaves the parent allocating too
    /// little vertical space — content clips and no scrollbar appears.
    ///
    /// Why not just <c>widestChild</c> (worst-case 1-column)? Because for tabs
    /// whose content fits within the viewport, this over-reports height and the
    /// parent reserves an oversized slot, leaving a visible gap below our items
    /// until a corrective layout pass settles (which is unreliable under engine
    /// load — see UserInterfaceManager's per-frame Measure/Arrange queues).
    ///
    /// Best heuristic: use the previous arranged width, then walk up the parent
    /// chain for an already-arranged ancestor and use its <c>Size.X</c>.
    /// That's the post-stretch width, which closely matches what we'll actually
    /// be arranged at.
    /// </summary>
    private float ResolveWrapWidth(float availableX, float widestChild)
    {
        var hasFiniteAvailable = !float.IsPositiveInfinity(availableX) && availableX > 0f;

        if (_lastArrangeWidth > 0f)
        {
            return hasFiniteAvailable
                ? Math.Min(availableX, _lastArrangeWidth)
                : _lastArrangeWidth;
        }

        // Fresh rows reuse the last real row width.
        if (SharedWidth is { Value: > 0f } shared)
        {
            return hasFiniteAvailable
                ? Math.Min(availableX, shared.Value)
                : shared.Value;
        }

        for (var p = Parent; p != null; p = p.Parent)
        {
            if (p.Size.X > 0)
            {
                return hasFiniteAvailable
                    ? Math.Min(availableX, p.Size.X)
                    : p.Size.X;
            }
        }

        // Nothing's been arranged yet anywhere up the chain — fall back to
        // widestChild (1-column worst case) so we never under-allocate.
        return widestChild;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var sep = ActualSeparation;
        var visible = Children.Where(c => c.Visible).ToList();
        if (visible.Count == 0)
            return Vector2.Zero;

        foreach (var child in visible)
            child.Measure(new Vector2(availableSize.X, float.PositiveInfinity));

        var widestChild = visible.Max(child => child.DesiredSize.X);
        var wrapWidth = Math.Max(ResolveWrapWidth(availableSize.X, widestChild), widestChild);

        var totalHeight = ComputeLayout(wrapWidth, sep, visible, null);
        var hasFiniteAvailable = !float.IsPositiveInfinity(availableSize.X) && availableSize.X > 0f;
        var desiredWidth = hasFiniteAvailable ? availableSize.X : wrapWidth;
        return new Vector2(desiredWidth, totalHeight);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var sep = ActualSeparation;
        var visible = Children.Where(c => c.Visible).ToList();
        ComputeLayout(finalSize.X, sep, visible, (child, x, y, w, h) =>
            child.Arrange(UIBox2.FromDimensions(x, y, w, h)));

        // If the actual arrange width disagrees with what we wrapped at during
        // measure, request a remeasure so DesiredSize updates with the real
        // column count next pass.
        if (!MathHelper.CloseTo(_lastArrangeWidth, finalSize.X, 0.5f))
        {
            _lastArrangeWidth = finalSize.X;
            InvalidateMeasure();
        }

        // Publish the real width for rebuilt rows.
        if (SharedWidth != null && finalSize.X > 0f)
            SharedWidth.Value = finalSize.X;

        return finalSize;
    }
}
