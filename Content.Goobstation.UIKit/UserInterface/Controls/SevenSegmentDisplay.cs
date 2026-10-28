using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

public sealed class SevenSegmentDisplay : Control
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

    private readonly List<Cell> _cells = new();

    private string _text = string.Empty;
    private int _minDigits;
    private int _segmentLength = 5;
    private int _segmentThickness = 2;
    private int _spacing = 2;
    private float _pixelScale = 2;

    public Color OnColor { get; set; } = Color.FromHex("#33ff33");

    public Color OffColor { get; set; } = Color.FromHex("#33ff3318");

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

    public int SegmentLength
    {
        get => _segmentLength;
        set
        {
            _segmentLength = Math.Max(1, value);
            InvalidateMeasure();
        }
    }

    public int SegmentThickness
    {
        get => _segmentThickness;
        set
        {
            _segmentThickness = Math.Max(1, value);
            InvalidateMeasure();
        }
    }

    public int Spacing
    {
        get => _spacing;
        set
        {
            _spacing = Math.Max(0, value);
            InvalidateMeasure();
        }
    }

    public float PixelScale
    {
        get => _pixelScale;
        set
        {
            _pixelScale = Math.Max(1, value);
            InvalidateMeasure();
        }
    }

    private int DigitWidth => _segmentThickness * 2 + _segmentLength;

    private int DigitHeight => _segmentThickness * 3 + _segmentLength * 2;

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return new Vector2(ArtWidth(), DigitHeight) * _pixelScale;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var unit = MathF.Max(1, MathF.Round(_pixelScale * UIScale));
        var origin = new Vector2(
            MathF.Floor((PixelSize.X - ArtWidth() * unit) / 2f),
            MathF.Floor((PixelSize.Y - DigitHeight * unit) / 2f));

        var x = 0;
        for (var i = 0; i < _cells.Count; i++)
        {
            if (i > 0)
                x += _spacing;

            var cell = _cells[i];
            switch (cell.Kind)
            {
                case CellKind.Digit:
                    DrawDigit(handle, origin, unit, x, cell.Segments);
                    x += DigitWidth;
                    break;
                case CellKind.Colon:
                    DrawColon(handle, origin, unit, x);
                    x += _segmentThickness;
                    break;
                case CellKind.Dot:
                    DrawPixels(handle, origin, unit, x, DigitHeight - _segmentThickness, _segmentThickness, _segmentThickness, OnColor);
                    x += _segmentThickness;
                    break;
            }
        }
    }

    private void DrawDigit(DrawingHandleScreen handle, Vector2 origin, float unit, int x, int segments)
    {
        var t = _segmentThickness;
        var l = _segmentLength;

        DrawHorizontal(handle, origin, unit, x + t, 0, segments, SegA);
        DrawHorizontal(handle, origin, unit, x + t, t + l, segments, SegG);
        DrawHorizontal(handle, origin, unit, x + t, t * 2 + l * 2, segments, SegD);
        DrawVertical(handle, origin, unit, x, t, segments, SegF);
        DrawVertical(handle, origin, unit, x + t + l, t, segments, SegB);
        DrawVertical(handle, origin, unit, x, t * 2 + l, segments, SegE);
        DrawVertical(handle, origin, unit, x + t + l, t * 2 + l, segments, SegC);
    }

    private void DrawHorizontal(DrawingHandleScreen handle, Vector2 origin, float unit, int x, int y, int segments, int segment)
    {
        var color = (segments & segment) != 0 ? OnColor : OffColor;
        for (var row = 0; row < _segmentThickness; row++)
        {
            var inset = Taper(row);
            DrawPixels(handle, origin, unit, x + inset, y + row, _segmentLength - inset * 2, 1, color);
        }
    }

    private void DrawVertical(DrawingHandleScreen handle, Vector2 origin, float unit, int x, int y, int segments, int segment)
    {
        var color = (segments & segment) != 0 ? OnColor : OffColor;
        for (var column = 0; column < _segmentThickness; column++)
        {
            var inset = Taper(column);
            DrawPixels(handle, origin, unit, x + column, y + inset, 1, _segmentLength - inset * 2, color);
        }
    }

    private void DrawColon(DrawingHandleScreen handle, Vector2 origin, float unit, int x)
    {
        var t = _segmentThickness;
        var l = _segmentLength;
        var offset = Math.Max(0, (l - t) / 2);

        DrawPixels(handle, origin, unit, x, t + offset, t, t, OnColor);
        DrawPixels(handle, origin, unit, x, t * 2 + l + offset, t, t, OnColor);
    }

    private int Taper(int index)
    {
        if (_segmentThickness < 3 || _segmentLength < 3)
            return 0;

        return index == 0 || index == _segmentThickness - 1 ? 1 : 0;
    }

    private static void DrawPixels(DrawingHandleScreen handle, Vector2 origin, float unit, int x, int y, int width, int height, Color color)
    {
        if (width <= 0 || height <= 0 || color.A <= 0)
            return;

        handle.DrawRect(UIBox2.FromDimensions(origin + new Vector2(x, y) * unit, new Vector2(width, height) * unit), color);
    }

    private int ArtWidth()
    {
        var width = 0;
        for (var i = 0; i < _cells.Count; i++)
        {
            if (i > 0)
                width += _spacing;

            width += _cells[i].Kind == CellKind.Digit ? DigitWidth : _segmentThickness;
        }

        return width;
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
