// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.IO;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Tools
{
    public sealed class Map
    {
        public Map(string path)
        {
            Path = path;

            using var reader = new StreamReader(path);
            var stream = new YamlStream();

            stream.Load(reader);

            Root = stream.Documents[0].RootNode;
            TilemapNode = (YamlMappingNode) Root["tilemap"];
            GridsNode = (YamlSequenceNode) Root["grids"];
            EntitiesNode = (YamlSequenceNode) Root["entities"];

            foreach (var entity in EntitiesNode)
            {
                var uid = uint.Parse(entity["uid"].AsString());
                if (uid >= NextAvailableEntityId)
                    NextAvailableEntityId = uid + 1;
                Entities[uid] = (YamlMappingNode) entity;
            }
        }

        // Core

        public string Path { get; }

        public YamlNode Root { get; }

        // Useful

        public YamlMappingNode TilemapNode { get; }

        public YamlSequenceNode GridsNode { get; }

        // Entities lookup

        private YamlSequenceNode EntitiesNode { get; }

        public Dictionary<uint, YamlMappingNode> Entities { get; } = new Dictionary<uint, YamlMappingNode>();

        public uint MaxId => Entities.Max(entry => entry.Key);

        public uint NextAvailableEntityId { get; set; }

        // ----

        public void Save(string fileName)
        {
            // Update entities node
            EntitiesNode.Children.Clear();
            foreach (var kvp in Entities)
                EntitiesNode.Add(kvp.Value);

            using var writer = new StreamWriter(fileName);
            var document = new YamlDocument(Root);
            var stream = new YamlStream(document);
            var emitter = new Emitter(writer);
            var fixer = new TypeTagPreserver(emitter);

            stream.Save(fixer, false);

            writer.Flush();
        }

        public void Save()
        {
            Save(Path);
        }
    }
}