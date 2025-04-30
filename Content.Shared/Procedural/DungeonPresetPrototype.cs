// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural;

[Prototype("dungeonPreset")]
public sealed partial class DungeonPresetPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// The room pack bounds we need to fill.
    /// </summary>
    [DataField("roomPacks", required: true)]
    public List<Box2i> RoomPacks = new();
}