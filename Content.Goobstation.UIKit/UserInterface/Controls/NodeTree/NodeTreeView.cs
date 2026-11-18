using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;

namespace Content.Goobstation.UIKit.UserInterface.Controls.NodeTree;

public sealed class NodeTreeView : Control
{
    public float Spacing { get; set; } = 150f;
    public Vector2 NodeSize { get; set; } = new(80, 80);
    public float MinZoom { get; set; } = 0.5f;
    public float MaxZoom { get; set; } = 2f;
    public float ZoomStep { get; set; } = 0.125f;
    public float EdgeThickness { get; set; } = 2f;
    public Color EdgeColor { get; set; } = Color.White;
    public Vector2 Home { get; set; }

    public float Zoom => _zoom;

    private readonly Dictionary<Control, Placement> _placements = new();
    private readonly List<NodeTreeEdge> _edges = new();
    private Vector2 _pan;
    private float _zoom = 1f;
    private bool _dragging;

    public NodeTreeView()
    {
        MouseFilter = MouseFilterMode.Stop;
        RectClipContent = true;
    }

    public void AddNode(Control node, Vector2 position)
    {
        _placements[node] = new Placement(position, true);
        AddChild(node);
    }

    public void AddDecoration(Control control, Vector2 position)
    {
        _placements[control] = new Placement(position, false);
        AddChild(control);
    }

    public NodeTreeEdge AddEdge(Control from, Control to)
    {
        var edge = new NodeTreeEdge(from, to) { Color = EdgeColor };
        _edges.Add(edge);
        return edge;
    }

    public void Clear()
    {
        _edges.Clear();
        _placements.Clear();
        RemoveAllChildren();
    }

    public void Recenter()
    {
        _pan = -Home * Spacing * _zoom;
        InvalidateArrange();
    }

    protected override void ChildRemoved(Control child)
    {
        base.ChildRemoved(child);

        _placements.Remove(child);
        _edges.RemoveAll(e => e.From == child || e.To == child);
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var nodeSize = NodeSize * _zoom;
        foreach (var child in Children)
        {
            var scaled = _placements.TryGetValue(child, out var placement) && placement.Scaled;
            child.Measure(scaled ? nodeSize : new Vector2(float.PositiveInfinity, float.PositiveInfinity));
        }

        return Vector2.Zero;
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var origin = finalSize / 2 + _pan;
        var nodeSize = NodeSize * _zoom;
        foreach (var child in Children)
        {
            if (!_placements.TryGetValue(child, out var placement))
            {
                child.Arrange(UIBox2.FromDimensions(Vector2.Zero, child.DesiredSize));
                continue;
            }

            var size = placement.Scaled ? nodeSize : child.DesiredSize;
            var center = origin + placement.Position * Spacing * _zoom;
            child.Arrange(UIBox2.FromDimensions(center - size / 2, size));
        }

        return finalSize;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var thickness = MathF.Max(1f, EdgeThickness * UIScale * _zoom);
        foreach (var edge in _edges)
        {
            if (!edge.From.Visible || !edge.To.Visible)
                continue;

            DrawElbow(handle, PixelCenter(edge.From), PixelCenter(edge.To), thickness, edge.Color);
        }
    }

    private static Vector2 PixelCenter(Control control)
    {
        return (Vector2) control.PixelPosition + (Vector2) control.PixelSize / 2;
    }

    private static void DrawElbow(DrawingHandleScreen handle, Vector2 from, Vector2 to, float thickness, Color color)
    {
        if (MathF.Abs(from.X - to.X) < 1f || MathF.Abs(from.Y - to.Y) < 1f)
        {
            DrawSegment(handle, from, to, thickness, color);
            return;
        }

        var midY = (from.Y + to.Y) / 2;
        var bendA = new Vector2(from.X, midY);
        var bendB = new Vector2(to.X, midY);
        DrawSegment(handle, from, bendA, thickness, color);
        DrawSegment(handle, bendA, bendB, thickness, color);
        DrawSegment(handle, bendB, to, thickness, color);
    }

    private static void DrawSegment(DrawingHandleScreen handle, Vector2 a, Vector2 b, float thickness, Color color)
    {
        var half = new Vector2(thickness / 2);
        handle.DrawRect(new UIBox2(Vector2.Min(a, b) - half, Vector2.Max(a, b) + half), color);
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _dragging = true;
        args.Handle();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function == EngineKeyFunctions.UIClick)
            _dragging = false;
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (!_dragging)
            return;

        _pan += args.Relative;
        InvalidateArrange();
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        var zoom = Math.Clamp(_zoom + MathF.Sign(args.Delta.Y) * ZoomStep, MinZoom, MaxZoom);
        args.Handle();

        if (MathHelper.CloseTo(zoom, _zoom))
            return;

        var cursor = args.RelativePosition - Size / 2;
        _pan = cursor - (cursor - _pan) * (zoom / _zoom);
        _zoom = zoom;
        InvalidateMeasure();
        InvalidateArrange();
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();

        _dragging = false;
    }

    private readonly record struct Placement(Vector2 Position, bool Scaled);
}

public sealed class NodeTreeEdge(Control from, Control to)
{
    public readonly Control From = from;
    public readonly Control To = to;
    public Color Color;
}
