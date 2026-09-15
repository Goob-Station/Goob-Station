// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.WildLandsFauna;

/// <summary>
/// NPC grazes grass tiles underfoot, replacing them with dirt and restoring hunger.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GrassGrazerComponent : Component
{
    [DataField]
    public HashSet<string> EdibleTiles = new()
    {
        "FloorPlanetGrass",
        "FloorGrassJungle",
        "FloorOskarrrDryGrass",
        "FloorGrass",
        "FloorGrassDark",
        "FloorGrassLight",
        "FloorAstroGrass",
        "FloorMowedAstroGrass",
        "FloorJungleAstroGrass",
        "FloorLightAstroGrass",
    };

    /// <summary>
    /// Tile placed after eating grass.
    /// </summary>
    [DataField]
    public string ReplaceTile = "FloorPlanetDirt";

    [DataField]
    public float EatInterval = 6f;

    [DataField]
    public float HungerRestore = 20f;

    [ViewVariables]
    public float Accumulator;
}
