// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Procedural;

namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Asteroid offered for the magnet.
/// </summary>
public record struct AsteroidOffering : ISalvageMagnetOffering
{
    public string Id;

    public DungeonConfig DungeonConfig;

    /// <summary>
    /// Calculated marker layers for the asteroid.
    /// </summary>
    public Dictionary<string, int> MarkerLayers;

    uint ISalvageMagnetOffering.Cost => 0; // DeltaV
}