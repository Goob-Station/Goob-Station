using Content.Shared.Parallax.Biomes.Layers;
using Robust.Shared.Noise;

namespace Content.Shared.Parallax.Biomes;

/// <summary>
/// Goob-specific methods for biome layer noise caching.
/// </summary>
public abstract partial class SharedBiomeSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BiomeComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<BiomeComponent> ent, ref ComponentStartup args)
    {
        // basically only for persistence loading
        CacheLayerNoises(ent.Comp);
    }

    /// <summary>
    /// Caches the noise for every layer with the current seed.
    /// </summary>
    protected void CacheLayerNoises(BiomeComponent comp)
    {
        comp.LayerNoises.Clear();
        AddLayerNoises(comp.LayerNoises, comp.Layers, comp.Seed);
    }

    /// <summary>
    /// Create a new cache of noises for each layer in a list with a given seed.
    /// </summary>
    public List<FastNoiseLite> CreateLayerNoises(IReadOnlyList<IBiomeLayer> layers, int seed)
    {
        var noises = new List<FastNoiseLite>(layers.Count);
        AddLayerNoises(noises, layers, seed);
        return noises;
    }

    /// <summary>
    /// Get or update the cached layer noises for a biome template and seed.
    /// Using a different seed than the last call will recalculate it, so do not interleave different biome seeds.
    /// </summary>
    public List<FastNoiseLite> GetTemplateNoises(BiomeTemplatePrototype template, int seed)
    {
        var noises = template.LayerNoises;
        if (template.LastCachedSeed == seed)
            return noises;

        noises.Clear();
        AddLayerNoises(noises, template.Layers, seed);
        template.LastCachedSeed = seed;
        return noises;
    }

    private void AddLayerNoises(List<FastNoiseLite> noises, IReadOnlyList<IBiomeLayer> layers, int seed)
    {
        foreach (var layer in layers)
        {
            noises.Add(GetNoise(layer.Noise, seed));
        }
    }
}
