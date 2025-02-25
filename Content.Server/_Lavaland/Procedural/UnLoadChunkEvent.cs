namespace Content.Server._Lavaland.Procedural;

/// <summary>
/// Lavaland: Raised when biome chunk is about to unload.
/// </summary>
public sealed class UnLoadChunkEvent : CancellableEntityEventArgs
{
    public Vector2i Chunk { get; set; }

    public UnLoadChunkEvent(Vector2i chunk)
    {
        Chunk = chunk;
    }
}
