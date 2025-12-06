// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
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
        var enumerator = new ChunkIndicesEnumerator(ent.Comp.LoadArea, SharedBiomeSystem.ChunkSize);

        while (enumerator.MoveNext(out var chunk))
        {
            var chunkOrigin = chunk * SharedBiomeSystem.ChunkSize;
            ent.Comp.LoadedChunks.Add(chunkOrigin.Value);
        }
    }

    private void OnChunkUnLoaded(Entity<BiomeOptimizeComponent> ent, ref UnLoadChunkEvent args)
    {
        if (ent.Comp.LoadedChunks.Contains(args.Chunk))
            args.Cancel();
    }

    private void OnChunkLoad(Entity<BiomeOptimizeComponent> ent, ref BeforeLoadChunkEvent args)
    {
        // We load only specified area.
        if (!ent.Comp.LoadArea.Contains(args.Chunk))
            args.Cancel();
    }
}
