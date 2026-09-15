// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.Map.Enumerators;

namespace Content.Server._Lavaland.Biome;

/// <summary>
/// System that stores already loaded chunks and stops BiomeSystem from unloading them.
/// This should finally prevent server from fucking dying because of 80 players on lavaland at the same time
/// </summary>
public sealed class LavalandMapOptimizationSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BiomeOptimizeComponent, UnLoadChunkEvent>(OnChunkUnLoaded);
        SubscribeLocalEvent<BiomeOptimizeComponent, BeforeLoadChunkEvent>(OnChunkLoad);
        SubscribeLocalEvent<BiomeOptimizeComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BiomeOptimizeComponent> ent, ref MapInitEvent args)
    {
        // Only pin the outpost — do not mark the whole planet as loaded (that forced RAM/CPU at roundstart).
        if (ent.Comp.PinnedArea.Equals(default) || ent.Comp.PinnedArea.IsEmpty())
            return;

        var enumerator = new ChunkIndicesEnumerator(ent.Comp.PinnedArea, SharedBiomeSystem.ChunkSize);

        while (enumerator.MoveNext(out var chunk))
        {
            var chunkOrigin = chunk * SharedBiomeSystem.ChunkSize;
            ent.Comp.LoadedChunks.Add(chunkOrigin.Value);
        }
    }

    private void OnChunkUnLoaded(Entity<BiomeOptimizeComponent> ent, ref UnLoadChunkEvent args)
    {
        // Keep outpost chunks warm; everything else unloads when players leave (normal biome behaviour).
        if (ent.Comp.LoadedChunks.Contains(args.Chunk))
            args.Cancelled = true;
    }

    private void OnChunkLoad(Entity<BiomeOptimizeComponent> ent, ref BeforeLoadChunkEvent args)
    {
        // Hard border of the planet — same as RestrictedRange.
        if (!ent.Comp.LoadArea.Contains(args.Chunk))
            args.Cancelled = true;
    }
}
