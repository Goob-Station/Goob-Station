using Content.Goobstation.Shared.Paint;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.Timing;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Goobstation.Client.Paint;

[UsedImplicitly]
public sealed partial class PaintBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private static readonly TimeSpan FlushInterval = TimeSpan.FromMilliseconds(50);
    private const int FlushLength = PaintBrush.MaxStrokeLength / 2;

    [Dependency] private IClyde _clyde = default!;
    [Dependency] private IClientGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private IUserInterfaceManager _uiManager = default!;

    private PaintSurfaceSystem _paint = default!;
    private PaintWindow? _window;
    private OwnedTexture? _texture;
    private Rgba32[] _buffer = Array.Empty<Rgba32>();
    private Vector2i _size;

    private readonly List<PendingStroke> _pending = new();
    private readonly List<Vector2i> _unsent = new();
    private readonly HashSet<int> _pixels = new();
    private Vector2i? _lastSent;
    private int _unsentLength;
    private TimeSpan _lastFlush;
    private int _sequence;
    private int _strokeId;

    private PaintWindowTool? _activeTool;
    private bool _strokeErase;
    private Vector2i _shapeStart;
    private Vector2i _shapeEnd;

    private EntityUid? _toolUid;
    private Color _toolColor;
    private bool _toolPicker;

    protected override void Open()
    {
        base.Open();

        _paint = EntMan.System<PaintSurfaceSystem>();
        _window = this.CreateWindow<PaintWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _window.FrameUpdated += OnFrameUpdated;
        _window.Canvas.StrokeStarted += OnStrokeStarted;
        _window.Canvas.StrokeMoved += OnStrokeMoved;
        _window.Canvas.StrokeEnded += OnStrokeEnded;
        _window.UndoPressed += Undo;
        _window.FinishPressed += Finish;
        _input.FirstChanceOnKeyEvent += OnKeyEvent;

        Rebuild();
        RefreshTool(true);
    }

    private void OnKeyEvent(KeyEventArgs keyEvent, KeyEventType type)
    {
        if (_window == null
            || type != KeyEventType.Down
            || !keyEvent.Control
            || keyEvent.Key != Keyboard.Key.Z
            || _uiManager.KeyboardFocused != null
            || !IsHoveringWindow())
            return;

        keyEvent.Handle();
        Undo();
    }

    private bool IsHoveringWindow()
    {
        for (var control = _uiManager.CurrentlyHovered; control != null; control = control.Parent)
        {
            if (control == _window)
                return true;
        }

        return false;
    }

    private void Undo()
    {
        if (_window is not { HasTool: true } || _activeTool != null)
            return;

        SendMessage(new PaintUndoMessage());
    }

    private void Finish(string title)
    {
        _window?.Canvas.EndStroke();
        SendMessage(new PaintFinishMessage(title));
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);

        if (message is not PaintStrokeAckMessage ack)
            return;

        foreach (var stroke in _pending)
        {
            if (stroke.Sequence == ack.Sequence)
                stroke.AckTick = ack.Tick;
        }
    }

    public void SurfaceUpdated(List<int>? chunks)
    {
        UpdateFinished();

        if (chunks == null || GetImage() is not { } image || image.Size != _size)
        {
            Rebuild();
            return;
        }

        foreach (var chunk in chunks)
        {
            PaintTextures.FillChunk(image, chunk, _buffer);
        }

        DrawOverlays(image);
        Upload();
    }

    private PaintImage? GetImage()
    {
        return EntMan.TryGetComponent<PaintSurfaceComponent>(Owner, out var surface) ? surface.Image : null;
    }

    private void UpdateFinished()
    {
        if (_window == null || !EntMan.TryGetComponent<PaintSurfaceComponent>(Owner, out var surface))
            return;

        _window.SetFinished(surface.Finished, surface.Title, surface.Artist);
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
    }

    private void Rebuild()
    {
        if (_window == null || GetImage() is not { } image)
            return;

        if (_texture == null || image.Size != _size)
        {
            _texture?.Dispose();
            _size = image.Size;
            _buffer = new Rgba32[_size.X * _size.Y];
            _texture = _clyde.CreateBlankTexture<Rgba32>(_size, "paint-window");
            _window.Canvas.ImageSize = _size;
            _window.Canvas.Texture = _texture;
        }

        PaintTextures.Fill(image, _buffer);
        DrawOverlays(image);
        Upload();
        UpdateFinished();
    }

    private void DrawOverlays(PaintImage image)
    {
        foreach (var stroke in _pending)
        {
            PaintLocal(image, stroke.Points, stroke.Shape, stroke.BrushSize, stroke.Style, stroke.Sequence, stroke.Erase, stroke.Color);
        }

        if (_window == null || _activeTool is not { } tool || ToShape(tool) is not { } shape)
            return;

        var points = new[] { _shapeStart, _shapeEnd };
        PaintLocal(image, points, shape, _window.BrushSize, _window.SelectedStyle, _sequence + 1, false, CurrentColor());
    }

    private void Upload()
    {
        _texture?.SetSubImage(Vector2i.Zero, _size, new ReadOnlySpan<Rgba32>(_buffer));
    }

    private void PaintLocal(
        PaintImage image,
        ReadOnlySpan<Vector2i> points,
        PaintShape shape,
        int brushSize,
        PaintBrushStyle style,
        int seed,
        bool erase,
        Color color)
    {
        _pixels.Clear();
        PaintBrush.Rasterize(points, shape, brushSize, style, seed, _size, _pixels);

        var rgba = PaintTextures.ToRgba(erase ? image.Palette[0] : color);
        foreach (var pixel in _pixels)
        {
            _buffer[pixel] = rgba;
        }
    }

    private void OnFrameUpdated()
    {
        RefreshTool(false);

        if (_unsent.Count > 0 && _timing.RealTime - _lastFlush >= FlushInterval)
            Flush();

        var dropped = false;
        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            if (_pending[i].AckTick is not { } tick || tick > _timing.LastRealTick)
                continue;

            _pending.RemoveAt(i);
            dropped = true;
        }

        if (dropped)
            Rebuild();
    }

    private void RefreshTool(bool force)
    {
        if (_window == null)
            return;

        Entity<PaintToolComponent>? tool = null;
        if (_player.LocalEntity is { } user)
            _paint.TryGetActiveTool(user, out tool);

        var uid = tool?.Owner;
        var picker = tool is { } current && _paint.AllowsColorPicker(current);
        var color = tool is { } colored ? _paint.GetToolColor(colored, Color.Black) : Color.Black;
        if (!force && uid == _toolUid && picker == _toolPicker && color == _toolColor)
            return;

        _toolUid = uid;
        _toolPicker = picker;
        _toolColor = color;
        _window.SetTool(tool?.Comp, picker, color);
    }

    private Color CurrentColor()
    {
        if (_window == null || _toolUid is not { } uid || !EntMan.TryGetComponent<PaintToolComponent>(uid, out var tool))
            return Color.Black;

        return _paint.GetToolColor((uid, tool), _window.PickedColor);
    }

    private static PaintShape? ToShape(PaintWindowTool tool)
    {
        return tool switch
        {
            PaintWindowTool.Line => PaintShape.Line,
            PaintWindowTool.Rectangle => PaintShape.Rectangle,
            PaintWindowTool.Ellipse => PaintShape.Ellipse,
            _ => null,
        };
    }

    private void OnStrokeStarted(Vector2i point)
    {
        if (_window == null)
            return;

        var tool = _window.SelectedTool;
        _activeTool = tool;

        if (tool == PaintWindowTool.Picker)
        {
            PickColor(point);
            return;
        }

        _strokeId++;

        if (ToShape(tool) != null)
        {
            _shapeStart = point;
            _shapeEnd = point;
            Rebuild();
            return;
        }

        _strokeErase = tool == PaintWindowTool.Eraser;
        _unsent.Clear();
        _unsentLength = 0;
        _lastSent = null;
        _lastFlush = _timing.RealTime;
        _unsent.Add(point);
        PaintSegment(point, point);
    }

    private void OnStrokeMoved(Vector2i point)
    {
        if (_activeTool is not { } tool)
            return;

        if (tool == PaintWindowTool.Picker)
        {
            PickColor(point);
            return;
        }

        if (ToShape(tool) != null)
        {
            _shapeEnd = point;
            Rebuild();
            return;
        }

        var previous = _unsent.Count > 0 ? _unsent[^1] : _lastSent ?? point;
        _unsent.Add(point);
        _unsentLength += Math.Max(Math.Abs(point.X - previous.X), Math.Abs(point.Y - previous.Y));
        PaintSegment(previous, point);

        if (_unsent.Count >= PaintBrush.MaxStrokePoints - 1 || _unsentLength >= FlushLength)
            Flush();
    }

    private void OnStrokeEnded()
    {
        if (_activeTool is not { } tool)
            return;

        _activeTool = null;

        if (ToShape(tool) is { } shape)
        {
            SendStroke(new[] { _shapeStart, _shapeEnd }, shape, false);
            Rebuild();
            return;
        }

        if (tool == PaintWindowTool.Picker)
            return;

        Flush();
        _lastSent = null;
    }

    private void PickColor(Vector2i point)
    {
        if (_window == null || point.X < 0 || point.Y < 0 || point.X >= _size.X || point.Y >= _size.Y)
            return;

        var pixel = _buffer[point.Y * _size.X + point.X];
        _window.PickedColor = new Color(pixel.R, pixel.G, pixel.B, (byte) 255);
    }

    private void PaintSegment(Vector2i from, Vector2i to)
    {
        if (_window == null || GetImage() is not { } image)
            return;

        var points = from == to ? new[] { to } : new[] { from, to };
        PaintLocal(image,
            points,
            PaintShape.Freehand,
            _window.BrushSize,
            _window.SelectedStyle,
            _sequence + 1,
            _strokeErase,
            CurrentColor());
        Upload();
    }

    private void Flush()
    {
        if (_window == null || _unsent.Count == 0)
            return;

        var offset = _lastSent != null ? 1 : 0;
        var points = new Vector2i[_unsent.Count + offset];
        if (_lastSent is { } last)
            points[0] = last;

        _unsent.CopyTo(points, offset);
        SendStroke(points, PaintShape.Freehand, _strokeErase);

        _lastSent = _unsent[^1];
        _unsent.Clear();
        _unsentLength = 0;
        _lastFlush = _timing.RealTime;
    }

    private void SendStroke(Vector2i[] points, PaintShape shape, bool erase)
    {
        if (_window == null)
            return;

        var stroke = new PendingStroke(++_sequence,
            CurrentColor(),
            _window.BrushSize,
            _window.SelectedStyle,
            shape,
            erase,
            points);
        _pending.Add(stroke);
        var strokeId = _strokeId;
        SendMessage(new PaintStrokeMessage(stroke.Sequence,
            _window.PickedColor,
            stroke.BrushSize,
            stroke.Style,
            stroke.Shape,
            stroke.Erase,
            strokeId,
            points));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _input.FirstChanceOnKeyEvent -= OnKeyEvent;
        _texture?.Dispose();
        _texture = null;
    }

    private sealed partial class PendingStroke(
        int sequence,
        Color color,
        int brushSize,
        PaintBrushStyle style,
        PaintShape shape,
        bool erase,
        Vector2i[] points)
    {
        public readonly int Sequence = sequence;
        public readonly Color Color = color;
        public readonly int BrushSize = brushSize;
        public readonly PaintBrushStyle Style = style;
        public readonly PaintShape Shape = shape;
        public readonly bool Erase = erase;
        public readonly Vector2i[] Points = points;
        public GameTick? AckTick;
    }
}
