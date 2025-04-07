// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural.Components;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.Map.Enumerators;

namespace Content.Server._Lavaland.Procedural.Systems;

/// <summary>
/// System that stores already loaded chunks and stops BiomeSystem from unloading them.
/// This should finally prevent server from fucking dying because of 80 players on lavaland at the same time
/// </summary>
public sealed class LavalandMapOptimizationSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LavalandMapComponent, UnLoadChunkEvent>(OnChunkUnLoaded);
        SubscribeLocalEvent<LavalandMapComponent, MapInitEvent>(OnChunkLoad);
    }

    private void OnChunkLoad(Entity<LavalandMapComponent> ent, ref MapInitEvent args)
    {
        var enumerator = new ChunkIndicesEnumerator(ent.Comp.LoadArea, SharedBiomeSystem.ChunkSize);

        while (enumerator.MoveNext(out var chunk))
        {
            var chunkOrigin = chunk * SharedBiomeSystem.ChunkSize;
            ent.Comp.LoadedChunks.Add(chunkOrigin.Value);
        }
    }

    private void OnChunkUnLoaded(Entity<LavalandMapComponent> ent, ref UnLoadChunkEvent args)
    {
        if (ent.Comp.LoadedChunks.Contains(args.Chunk))
        {
            args.Cancel();
        }
    }
}