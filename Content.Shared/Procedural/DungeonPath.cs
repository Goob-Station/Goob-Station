// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural;

/// <summary>
/// Connects 2 dungeon rooms.
/// </summary>
public sealed record DungeonPath(string Tile, string Wall, HashSet<Vector2i> Tiles)
{
    public string Tile = Tile;
    public string Wall = Wall;
}