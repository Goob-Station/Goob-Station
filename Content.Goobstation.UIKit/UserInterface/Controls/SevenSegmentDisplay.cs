using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

public sealed partial class SevenSegmentDisplay : Control
{
    private const int SegA = 1 << 0;
    private const int SegB = 1 << 1;
    private const int SegC = 1 << 2;
    private const int SegD = 1 << 3;
    private const int SegE = 1 << 4;
    private const int SegF = 1 << 5;
    private const int SegG = 1 << 6;

    private static readonly Dictionary<char, int> Glyphs = new()
    {
        ['0'] = SegA | SegB | SegC | SegD | SegE | SegF,
        ['1'] = SegB | SegC,
        ['2'] = SegA | SegB | SegG | SegE | SegD,
        ['3'] = SegA | SegB | SegG | SegC | SegD,
        ['4'] = SegF | SegG | SegB | SegC,
        ['5'] = SegA | SegF | SegG | SegC | SegD,
        ['6'] = SegA | SegF | SegG | SegE | SegD | SegC,
        ['7'] = SegA | SegB | SegC,
        ['8'] = SegA | SegB | SegC | SegD | SegE | SegF | SegG,
        ['9'] = SegA | SegB | SegC | SegD | SegF | SegG,
        ['-'] = SegG,
        ['_'] = SegD,
        ['='] = SegD | SegG,
        ['?'] = SegA | SegB | SegG | SegE,
        ['A'] = SegA | SegB | SegC | SegE | SegF | SegG,
        ['b'] = SegC | SegD | SegE | SegF | SegG,
        ['C'] = SegA | SegD | SegE | SegF,
        ['c'] = SegD | SegE | SegG,
        ['d'] = SegB | SegC | SegD | SegE | SegG,
        ['E'] = SegA | SegD | SegE | SegF | SegG,
        ['F'] = SegA | SegE | SegF | SegG,
        ['G'] = SegA | SegC | SegD | SegE | SegF,
        ['H'] = SegB | SegC | SegE | SegF | SegG,
        ['h'] = SegC | SegE | SegF | SegG,
        ['I'] = SegE | SegF,
        ['J'] = SegB | SegC | SegD | SegE,
        ['L'] = SegD | SegE | SegF,
        ['n'] = SegC | SegE | SegG,
        ['O'] = SegA | SegB | SegC | SegD | SegE | SegF,
        ['o'] = SegC | SegD | SegE | SegG,
        ['P'] = SegA | SegB | SegE | SegF | SegG,
        ['q'] = SegA | SegB | SegC | SegF | SegG,
        ['r'] = SegE | SegG,
        ['S'] = SegA | SegF | SegG | SegC | SegD,
        ['t'] = SegD | SegE | SegF | SegG,
        ['U'] = SegB | SegC | SegD | SegE | SegF,
        ['u'] = SegC | SegD | SegE,
        ['y'] = SegB | SegC | SegD | SegF | SegG,
        ['Z'] = SegA | SegB | SegG | SegE | SegD,
    };

    private const string TexturePath = "/Textures/_Goobstation/Interface/SevenSegment";
    private static readonly string[] SegmentNames = { "a", "b", "c", "d", "e", "f", "g" };

    [Dependency] private IResourceCache _cache = default!;

    private readonly List<Cell> _cells = new();
    private readonly Texture[] _segments = new Texture[7];
    private readonly Texture _colon;
    private readonly Texture _decimal;

    private string _text = string.Empty;
    private int _minDigits;
    private float _spacing = 6;
    private float _scale = 1;

    public Color SegmentColor { get; set; } = Color.FromHex("#33ff33");

    public float OffAlpha { get; set; } = 0.02f;

    public SevenSegmentDisplay()
    {
        IoCManager.InjectDependencies(this);

        for (var i = 0; i < SegmentNames.Length; i++)
        {
            _segments[i] = LoadTexture($"segment_{SegmentNames[i]}");
        }

        _colon = LoadTexture("colon");
        _decimal = LoadTexture("decimal");
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value)
                return;

            _text = value;
            RebuildCells();
        }
    }

    public int MinDigits
    {
        get => _minDigits;
        set
        {
            _minDigits = Math.Max(0, value);
            RebuildCells();
        }
    }

    public float Spacing
    {
        get => _spacing;
        set
        {
            _spacing = Math.Max(0, value);
            InvalidateMeasure();
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = Math.Max(0.1f, value);
            InvalidateMeasure();
        }
    }

    private Vector2 DigitSize => _segments[0].Size;

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return new Vector2(ArtWidth(), DigitSize.Y) * _scale;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var scale = _scale * UIScale;
        var origin = new Vector2(
            MathF.Floor((PixelSize.X - ArtWidth() * scale) / 2f),
            MathF.Floor((PixelSize.Y - DigitSize.Y * scale) / 2f));

        var x = 0f;
        for (var i = 0; i < _cells.Count; i++)
        {
            if (i > 0)
                x += _spacing;

            var cell = _cells[i];
            var position = origin + new Vector2(x * scale, 0);
            switch (cell.Kind)
            {
                case CellKind.Digit:
                    var off = SegmentColor.WithAlpha(SegmentColor.A * OffAlpha);
                    for (var segment = 0; segment < _segments.Length; segment++)
                    {
                        var lit = (cell.Segments & (1 << segment)) != 0;
                        DrawCell(handle, _segments[segment], position, scale, lit ? SegmentColor : off);
                    }
                    break;
                case CellKind.Colon:
                    DrawCell(handle, _colon, position, scale, SegmentColor);
                    break;
                case CellKind.Dot:
                    DrawCell(handle, _decimal, position, scale, SegmentColor);
                    break;
            }

            x += CellWidth(cell.Kind);
        }
    }

    private void DrawCell(DrawingHandleScreen handle, Texture texture, Vector2 position, float scale, Color color)
    {
        if (color.A <= 0)
            return;

        handle.DrawTextureRect(texture, UIBox2.FromDimensions(position, (Vector2) texture.Size * scale), color);
    }

    private float CellWidth(CellKind kind)
    {
        return kind switch
        {
            CellKind.Colon => _colon.Width,
            CellKind.Dot => _decimal.Width,
            _ => DigitSize.X,
        };
    }

    private float ArtWidth()
    {
        var width = 0f;
        for (var i = 0; i < _cells.Count; i++)
        {
            if (i > 0)
                width += _spacing;

            width += CellWidth(_cells[i].Kind);
        }

        return width;
    }

    private Texture LoadTexture(string name)
    {
        return _cache.GetResource<TextureResource>($"{TexturePath}/{name}.png").Texture;
    }

    private void RebuildCells()
    {
        _cells.Clear();

        var digits = 0;
        foreach (var c in _text)
        {
            if (c != ':' && c != '.')
                digits++;
        }

        for (var i = digits; i < _minDigits; i++)
        {
            _cells.Add(new Cell(CellKind.Digit, 0));
        }

        foreach (var c in _text)
        {
            switch (c)
            {
                case ':':
                    _cells.Add(new Cell(CellKind.Colon, 0));
                    break;
                case '.':
                    _cells.Add(new Cell(CellKind.Dot, 0));
                    break;
                default:
                    _cells.Add(new Cell(CellKind.Digit, GlyphFor(c)));
                    break;
            }
        }

        InvalidateMeasure();
    }

    private static int GlyphFor(char c)
    {
        if (Glyphs.TryGetValue(c, out var glyph))
            return glyph;

        if (Glyphs.TryGetValue(char.ToUpperInvariant(c), out glyph))
            return glyph;

        return Glyphs.TryGetValue(char.ToLowerInvariant(c), out glyph) ? glyph : 0;
    }

    private enum CellKind : byte
    {
        Digit,
        Colon,
        Dot,
    }

    private readonly record struct Cell(CellKind Kind, int Segments);
}
