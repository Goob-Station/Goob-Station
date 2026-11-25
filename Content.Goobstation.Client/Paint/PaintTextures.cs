using Content.Goobstation.Shared.Paint;
using Robust.Client.Graphics;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Goobstation.Client.Paint;

public static class PaintTextures
{
    public static Rgba32 ToRgba(Color color)
    {
        return new Rgba32(color.RByte, color.GByte, color.BByte, color.AByte);
    }

    public static void Fill(PaintImage image, Rgba32[] buffer)
    {
        var palette = new Rgba32[image.Palette.Count];
        for (var i = 0; i < palette.Length; i++)
        {
            palette[i] = ToRgba(image.Palette[i]);
        }

        for (var i = 0; i < image.Pixels.Length; i++)
        {
            var entry = image.Pixels[i];
            buffer[i] = entry < palette.Length ? palette[entry] : palette[0];
        }
    }

    public static void FillChunk(PaintImage image, int chunk, Rgba32[] buffer)
    {
        var bounds = PaintChunks.Bounds(image.Size, chunk);
        for (var y = bounds.Top; y < bounds.Bottom; y++)
        {
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                var index = y * image.Width + x;
                buffer[index] = ToRgba(image.GetPixel(index));
            }
        }
    }

    public static void UploadChunk(OwnedTexture texture, PaintImage image, int chunk, Rgba32[] scratch)
    {
        var bounds = PaintChunks.Bounds(image.Size, chunk);
        var i = 0;
        for (var y = bounds.Top; y < bounds.Bottom; y++)
        {
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                scratch[i++] = ToRgba(image.GetPixel(y * image.Width + x));
            }
        }

        texture.SetSubImage(bounds.TopLeft, bounds.Size, new ReadOnlySpan<Rgba32>(scratch, 0, i));
    }

    public static OwnedTexture Create(IClyde clyde, PaintImage image, string name)
    {
        var texture = clyde.CreateBlankTexture<Rgba32>(image.Size, name);
        var buffer = new Rgba32[image.Width * image.Height];
        Fill(image, buffer);
        texture.SetSubImage(Vector2i.Zero, image.Size, new ReadOnlySpan<Rgba32>(buffer));
        return texture;
    }
}
