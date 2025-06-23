namespace Content.Server._Lavaland.Biome;

/// <summary>
/// Optimization component that stops the planet from unloading chunks, or loading chunks that aren't in the area.
/// Add this to planet maps that have known borders to reduce lag.
/// </summary>
[RegisterComponent]
public sealed partial class BiomeOptimizeComponent : Component
{
    /// <summary>
    /// Chunks in this area are always loaded
    /// </summary>
    [ViewVariables]
    public Box2 LoadArea;

    /// <summary>
    /// Already loaded chunks
    /// </summary>
    [DataField("loadedChunks")]
    public HashSet<Vector2i> LoadedChunks = new();
}
