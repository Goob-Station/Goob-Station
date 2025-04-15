// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Maps;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Tiles
{
    /// <summary>
    /// This gives items floor tile behavior, but it doesn't have to be a literal floor tile.
    /// A lot of materials use this too. Note that the AfterInteract will fail without a stack component on the item.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class FloorTileComponent : Component
    {
        [DataField("outputs", customTypeSerializer: typeof(PrototypeIdListSerializer<ContentTileDefinition>))]
        public List<string>? OutputTiles;

        [DataField("placeTileSound")] public SoundSpecifier PlaceTileSound =
            new SoundPathSpecifier("/Audio/Items/genhit.ogg")
            {
                Params = AudioParams.Default.WithVariation(0.125f),
            };
    }
}