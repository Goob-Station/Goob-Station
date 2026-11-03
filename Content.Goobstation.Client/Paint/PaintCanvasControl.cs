using System.Numerics;
using Content.Goobstation.Shared.Paint;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;

namespace Content.Goobstation.Client.Paint;

public sealed class PaintCanvasControl : Control
{
    private static readonly Color BackdropColor = Color.FromHex("#1b1b1e");
    private static readonly Color BorderColor = Color.FromHex("#5a5a66");
    private static readonly Color CursorColor = Color.FromHex("#ffffffaa");

    public Texture? Texture;
    public Vector2i ImageSize = new(64, 64);
    public bool Interactive = true;

    public event Action<Vector2i>? StrokeStarted;
    public event Action<Vector2i>? StrokeMoved;
    public event Action? StrokeEnded;

    private bool _drawing;
    private Vector2i _last;
    private Vector2i? _hover;
    private int _brushSize = 1;
    private PaintBrushStyle _brushStyle = PaintBrushStyle.Round;
    private readonly HashSet<Vector2i> _footprint = new();
    private readonly List<(Vector2i From, Vector2i To)> _outline = new();

    public PaintCanvasControl()
    {
        MouseFilter = MouseFilterMode.Stop;
        RectClipContent = true;
        RebuildOutline();
    }

    private UIBox2 ImageRect()
    {
        var fit = MathF.Min(PixelSize.X / (float) ImageSize.X, PixelSize.Y / (float) ImageSize.Y);
        var scale = fit >= 1f ? MathF.Floor(fit) : fit;
        var size = new Vector2(ImageSize.X, ImageSize.Y) * scale;
        var topLeft = (new Vector2(PixelSize.X, PixelSize.Y) - size) / 2f;
        return UIBox2.FromDimensions(topLeft, size);
    }

    private Vector2i ToImage(Vector2 pixelPosition)
    {
        var rect = ImageRect();
        var relative = (pixelPosition - rect.TopLeft) / rect.Size * new Vector2(ImageSize.X, ImageSize.Y);
        return new Vector2i((int) MathF.Floor(relative.X), (int) MathF.Floor(relative.Y));
    }

    private bool InImage(Vector2i point)
    {
        return point.X >= 0 && point.Y >= 0 && point.X < ImageSize.X && point.Y < ImageSize.Y;
    }

    private Vector2i ClampToMargin(Vector2i point)
    {
        var margin = PaintBrush.PointMargin - 1;
        return new Vector2i(Math.Clamp(point.X, -margin, ImageSize.X + margin - 1),
            Math.Clamp(point.Y, -margin, ImageSize.Y + margin - 1));
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        handle.DrawRect(PixelSizeBox, BackdropColor);

        var rect = ImageRect();
        if (Texture != null)
            handle.DrawTextureRect(Texture, rect);

        handle.DrawRect(rect, BorderColor, false);

        if (!Interactive || _hover is not { } hover)
            return;

        var pixel = rect.Size / new Vector2(ImageSize.X, ImageSize.Y);
        var origin = rect.TopLeft + new Vector2(hover.X, hover.Y) * pixel;
        foreach (var (from, to) in _outline)
        {
            handle.DrawLine(origin + new Vector2(from.X, from.Y) * pixel, origin + new Vector2(to.X, to.Y) * pixel, CursorColor);
        }
    }

    public void SetBrush(int size, PaintBrushStyle style)
    {
        if (size == _brushSize && style == _brushStyle && _outline.Count > 0)
            return;

        _brushSize = size;
        _brushStyle = style;
        RebuildOutline();
    }

    private void RebuildOutline()
    {
        var style = _brushStyle == PaintBrushStyle.Spray ? PaintBrushStyle.Round : _brushStyle;
        var center = new Vector2i(_brushSize + 1, _brushSize + 1);
        var bounds = center * 2 + Vector2i.One;
        var pixels = new HashSet<int>();
        PaintBrush.Rasterize(new[] { center }, PaintShape.Freehand, _brushSize, style, 0, bounds, pixels);

        _footprint.Clear();
        foreach (var pixel in pixels)
        {
            _footprint.Add(new Vector2i(pixel % bounds.X - center.X, pixel / bounds.X - center.Y));
        }

        _outline.Clear();
        foreach (var cell in _footprint)
        {
            if (!_footprint.Contains(cell + new Vector2i(0, -1)))
                _outline.Add((cell, cell + new Vector2i(1, 0)));

            if (!_footprint.Contains(cell + new Vector2i(0, 1)))
                _outline.Add((cell + new Vector2i(0, 1), cell + new Vector2i(1, 1)));

            if (!_footprint.Contains(cell + new Vector2i(-1, 0)))
                _outline.Add((cell, cell + new Vector2i(0, 1)));

            if (!_footprint.Contains(cell + new Vector2i(1, 0)))
                _outline.Add((cell + new Vector2i(1, 0), cell + new Vector2i(1, 1)));
        }
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (!Interactive || args.Function != EngineKeyFunctions.UIClick)
            return;

        var point = ToImage(args.RelativePixelPosition);
        if (!InImage(point))
            return;

        _drawing = true;
        _last = point;
        StrokeStarted?.Invoke(point);
        args.Handle();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function == EngineKeyFunctions.UIClick)
            EndStroke();
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        var point = ToImage(args.RelativePixelPosition);
        _hover = InImage(point) ? point : null;

        if (!_drawing)
            return;

        point = ClampToMargin(point);
        if (point == _last)
            return;

        _last = point;
        StrokeMoved?.Invoke(point);
    }

    protected override void MouseExited()
    {
        base.MouseExited();

        _hover = null;
    }

    public void EndStroke()
    {
        if (!_drawing)
            return;

        _drawing = false;
        StrokeEnded?.Invoke();
    }
}
