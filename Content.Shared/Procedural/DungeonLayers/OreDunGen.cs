// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonLayers;

/// <summary>
/// Generates veins inside of the specified dungeon.
/// </summary>
/// <remarks>
/// Generates on top of existing entities for sanity reasons moreso than performance.
/// </remarks>
[Virtual]
public partial class OreDunGen : IDunGenLayer
{
    /// <summary>
    /// If the vein generation should occur on top of existing entities what are we replacing.
    /// </summary>
    [DataField]
    public EntProtoId? Replacement;

    /// <summary>
    /// Entity to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Entity;

    /// <summary>
    /// Maximum amount of group spawns
    /// </summary>
    [DataField]
    public int Count = 10;

    /// <summary>
    /// Minimum entities to spawn in one group.
    /// </summary>
    [DataField]
    public int MinGroupSize = 1;

    /// <summary>
    /// Maximum entities to spawn in one group.
    /// </summary>
    [DataField]
    public int MaxGroupSize = 1;
}