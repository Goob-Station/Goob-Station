// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Content.Shared.Decals.DecalGridComponent;

namespace Content.Shared.Decals
{
    [RegisterComponent]
    [Access(typeof(SharedDecalSystem))]
    [NetworkedComponent]
    public sealed partial class DecalGridComponent : Component
    {
        [Access(Other = AccessPermissions.ReadExecute)]
        [DataField(serverOnly: true)]
        public DecalGridChunkCollection ChunkCollection = new(new ());

        /// <summary>
        ///     Dictionary mapping decals to their corresponding grid chunks.
        /// </summary>
        public readonly Dictionary<uint, Vector2i> DecalIndex = new();

        /// <summary>
        ///     Tick at which PVS was last toggled. Ensures that all players receive a full update when toggling PVS.
        /// </summary>
        public GameTick ForceTick { get; set; }

        [DataDefinition]
        [Serializable, NetSerializable]
        public sealed partial class DecalChunk
        {
            [IncludeDataField(customTypeSerializer:typeof(DictionarySerializer<uint, Decal>))]
            public Dictionary<uint, Decal> Decals;

            [NonSerialized]
            public GameTick LastModified;

            public DecalChunk()
            {
                Decals = new();
            }

            public DecalChunk(Dictionary<uint, Decal> decals)
            {
                Decals = decals;
            }

            public DecalChunk(DecalChunk chunk)
            {
                // decals are readonly, so this should be fine.
                Decals = chunk.Decals.ShallowClone();
                LastModified = chunk.LastModified;
            }
        }

        [DataRecord, Serializable, NetSerializable]
        public partial record DecalGridChunkCollection(Dictionary<Vector2i, DecalChunk> ChunkCollection)
        {
            public uint NextDecalId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class DecalGridState(Dictionary<Vector2i, DecalChunk> chunks) : ComponentState
    {
        public Dictionary<Vector2i, DecalChunk> Chunks = chunks;
    }

    [Serializable, NetSerializable]
    public sealed class DecalGridDeltaState(Dictionary<Vector2i, DecalChunk> modifiedChunks, HashSet<Vector2i> allChunks)
        : ComponentState, IComponentDeltaState<DecalGridState>
    {
        public Dictionary<Vector2i, DecalChunk> ModifiedChunks = modifiedChunks;
        public HashSet<Vector2i> AllChunks = allChunks;

        public void ApplyToFullState(DecalGridState state)
        {
            foreach (var key in state.Chunks.Keys)
            {
                if (!AllChunks!.Contains(key))
                    state.Chunks.Remove(key);
            }

            foreach (var (chunk, data) in ModifiedChunks)
            {
                state.Chunks[chunk] = new(data);
            }
        }

        public DecalGridState CreateNewFullState(DecalGridState state)
        {
            var chunks = new Dictionary<Vector2i, DecalChunk>(state.Chunks.Count);

            foreach (var (chunk, data) in ModifiedChunks)
            {
                chunks[chunk] = new(data);
            }

            foreach (var (chunk, data) in state.Chunks)
            {
                if (AllChunks!.Contains(chunk))
                    chunks.TryAdd(chunk, new(data));
            }
            return new DecalGridState(chunks);
        }
    }
}