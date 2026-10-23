namespace Content.Goobstation.Shared.Paint;

public static class PaintChunks
{
    public const int ChunkSize = 16;

    public static Vector2i ChunkGrid(Vector2i size)
    {
        return new Vector2i((size.X + ChunkSize - 1) / ChunkSize, (size.Y + ChunkSize - 1) / ChunkSize);
    }

    public static int Count(Vector2i size)
    {
        var grid = ChunkGrid(size);
        return grid.X * grid.Y;
    }

    public static int ChunkOf(Vector2i size, int pixel)
    {
        var grid = ChunkGrid(size);
        var x = pixel % size.X / ChunkSize;
        var y = pixel / size.X / ChunkSize;
        return y * grid.X + x;
    }

    public static UIBox2i Bounds(Vector2i size, int chunk)
    {
        var grid = ChunkGrid(size);
        var left = chunk % grid.X * ChunkSize;
        var top = chunk / grid.X * ChunkSize;
        return new UIBox2i(left, top, Math.Min(left + ChunkSize, size.X), Math.Min(top + ChunkSize, size.Y));
    }

    public static byte[] Read(byte[] pixels, Vector2i size, int chunk)
    {
        var bounds = Bounds(size, chunk);
        var data = new byte[bounds.Width * bounds.Height];
        for (var row = 0; row < bounds.Height; row++)
        {
            Array.Copy(pixels, (bounds.Top + row) * size.X + bounds.Left, data, row * bounds.Width, bounds.Width);
        }

        return data;
    }

    public static void Write(byte[] pixels, Vector2i size, int chunk, byte[] data)
    {
        if (chunk < 0 || chunk >= Count(size) || pixels.Length != size.X * size.Y)
            return;

        var bounds = Bounds(size, chunk);
        if (data.Length != bounds.Width * bounds.Height)
            return;

        for (var row = 0; row < bounds.Height; row++)
        {
            Array.Copy(data, row * bounds.Width, pixels, (bounds.Top + row) * size.X + bounds.Left, bounds.Width);
        }
    }
}
