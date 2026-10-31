using System.IO;
using System.IO.Compression;
using Content.Goobstation.Shared.Paint;

namespace Content.Goobstation.Server.Paint;

public static class PaintImageCodec
{
    private const byte Version = 1;
    private const int MaxDimension = 1024;

    public static string Encode(PaintImage image)
    {
        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, true))
        using (var writer = new BinaryWriter(deflate))
        {
            writer.Write(Version);
            writer.Write((ushort) image.Width);
            writer.Write((ushort) image.Height);
            writer.Write((ushort) image.Palette.Count);
            foreach (var color in image.Palette)
            {
                writer.Write(color.RByte);
                writer.Write(color.GByte);
                writer.Write(color.BByte);
            }

            writer.Write(image.Pixels);
        }

        return Convert.ToBase64String(output.ToArray());
    }

    public static PaintImage? Decode(string encoded)
    {
        try
        {
            using var input = new MemoryStream(Convert.FromBase64String(encoded));
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var reader = new BinaryReader(deflate);

            if (reader.ReadByte() != Version)
                return null;

            int width = reader.ReadUInt16();
            int height = reader.ReadUInt16();
            int paletteCount = reader.ReadUInt16();
            if (width <= 0 || height <= 0 || width > MaxDimension || height > MaxDimension
                || paletteCount <= 0 || paletteCount > PaintImage.MaxPaletteSize)
                return null;

            var palette = new List<Color>(paletteCount);
            for (var i = 0; i < paletteCount; i++)
            {
                palette.Add(new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), (byte) 255));
            }

            var pixels = reader.ReadBytes(width * height);
            if (pixels.Length != width * height)
                return null;

            return new PaintImage(width, height, palette, pixels);
        }
        catch (Exception e) when (e is FormatException or InvalidDataException or EndOfStreamException)
        {
            return null;
        }
    }
}
