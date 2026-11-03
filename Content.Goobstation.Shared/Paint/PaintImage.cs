using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Paint;

[Serializable, NetSerializable]
public sealed class PaintImage
{
    public const int MaxPaletteSize = 256;

    public int Width;
    public int Height;
    public List<Color> Palette;
    public byte[] Pixels;

    public PaintImage(int width, int height, Color background)
    {
        Width = width;
        Height = height;
        Palette = new List<Color> { Quantize(background) };
        Pixels = new byte[width * height];
    }

    public PaintImage(int width, int height, List<Color> palette, byte[] pixels)
    {
        Width = width;
        Height = height;
        Palette = palette;
        Pixels = pixels;
    }

    public Vector2i Size => new(Width, Height);

    public Color GetPixel(int index)
    {
        var entry = Pixels[index];
        return entry < Palette.Count ? Palette[entry] : Palette[0];
    }

    public byte FindOrAddColor(Color color, out bool added)
    {
        added = false;
        color = Quantize(color);

        var best = 0;
        var bestDistance = int.MaxValue;
        for (var i = 0; i < Palette.Count; i++)
        {
            var distance = Distance(Palette[i], color);
            if (distance == 0)
                return (byte) i;

            if (distance >= bestDistance)
                continue;

            best = i;
            bestDistance = distance;
        }

        if (Palette.Count >= MaxPaletteSize)
            return (byte) best;

        Palette.Add(color);
        added = true;
        return (byte) (Palette.Count - 1);
    }

    public PaintImage Clone()
    {
        return new PaintImage(Width, Height, new List<Color>(Palette), (byte[]) Pixels.Clone());
    }

    public static Color Quantize(Color color)
    {
        return new Color(color.RByte, color.GByte, color.BByte, (byte) 255);
    }

    private static int Distance(Color a, Color b)
    {
        var r = a.RByte - b.RByte;
        var g = a.GByte - b.GByte;
        var bl = a.BByte - b.BByte;
        return r * r + g * g + bl * bl;
    }
}
