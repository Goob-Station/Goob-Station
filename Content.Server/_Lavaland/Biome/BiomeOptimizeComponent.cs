// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Lavaland.Biome;

/// <summary>
/// Restricts biome chunk loading to <see cref="LoadArea"/> (planet border).
/// Optionally pins <see cref="PinnedArea"/> chunks so they never unload (outpost).
/// Distant terrain otherwise streams in/out with players like normal biomes.
/// </summary>
[RegisterComponent]
public sealed partial class BiomeOptimizeComponent : Component
{
    /// <summary>
    /// Max area where biome chunks may load (restricted planet range).
    /// </summary>
    [DataField]
    public Box2 LoadArea;

    /// <summary>
    /// Small area around the outpost that stays preloaded and is never unloaded.
    /// Empty / zero-size means nothing is pinned — full lazy load.
    /// </summary>
    [DataField]
    public Box2 PinnedArea;

    [ViewVariables]
    public HashSet<Vector2i> LoadedChunks = new();
}
