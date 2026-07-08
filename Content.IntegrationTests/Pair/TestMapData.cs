// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.IntegrationTests.Pair;

/// <summary>
/// Simple data class that stored information about a map being used by a test.
/// </summary>
public sealed class TestMapData
{
    public EntityUid MapUid { get; set; }
    public Entity<MapGridComponent> Grid;
    public MapId MapId;
    public EntityCoordinates GridCoords { get; set; }
    public MapCoordinates MapCoords { get; set; }
    public TileRef Tile { get; set; }

    // Client-side uids
    public EntityUid CMapUid { get; set; }
    public EntityUid CGridUid { get; set; }
    public EntityCoordinates CGridCoords { get; set; }
}