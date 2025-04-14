// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Serialization;

namespace Content.Shared.Storage.Components
{
    [Serializable, NetSerializable]
    public enum StorageMapVisuals : sbyte
    {
        InitLayers,
        LayerChanged,
    }

    [Serializable]
    [DataDefinition]
    public sealed partial class SharedMapLayerData
    {
        public string Layer = string.Empty;

        [DataField(required: true)]
        public EntityWhitelist? Whitelist { get; set; }

        /// <summary>
        ///     Minimal amount of entities that are valid for whitelist.
        ///     If it's smaller than minimal amount, layer will be hidden.
        /// </summary>
        [DataField]
        public int MinCount = 1;

        /// <summary>
        ///     Max amount of entities that are valid for whitelist.
        ///     If it's bigger than max amount, layer will be hidden.
        /// </summary>
        [DataField]
        public int MaxCount = int.MaxValue;
    }

    [Serializable, NetSerializable]
    public sealed class ShowLayerData : ICloneable
    {
        public readonly IReadOnlyList<string> QueuedEntities;

        public ShowLayerData()
        {
            QueuedEntities = new List<string>();
        }

        public ShowLayerData(IReadOnlyList<string> other)
        {
            QueuedEntities = other;
        }

        public object Clone()
        {
            // QueuedEntities should never be getting modified after this object is created.
            return this;
        }
    }
}