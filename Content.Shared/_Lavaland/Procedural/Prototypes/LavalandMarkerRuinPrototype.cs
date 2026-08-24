// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// Spawns a simple marker on picked coordinates and ensures that nothing intersects the Boundary box around it.
/// Generated after grid and dungeon ruins
/// </summary>
[Prototype]
public sealed partial class LavalandMarkerRuinPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public Vector2i Boundary { get; private set; }

    [DataField(required: true)]
    public EntProtoId SpawnedMarker;

    [DataField]
    public int SpawnAttempts = 8;

    [DataField(required: true)]
    public int Priority = int.MinValue;
}
