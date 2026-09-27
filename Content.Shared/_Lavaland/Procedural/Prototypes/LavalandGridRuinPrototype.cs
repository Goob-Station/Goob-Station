// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// Contains information about Lavaland ruin configuration.
/// </summary>
[Prototype]
public sealed partial class LavalandGridRuinPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public LocId Name = "lavaland-ruin-unknown";

    [DataField(required: true)]
    public ResPath Path;

    [DataField]
    public int SpawnAttempts = 8;

    /// <summary>
    /// Overrides the pool's MinDistance for this specific ruin.
    /// </summary>
    [DataField]
    public int? MinDistance;

    /// <summary>
    /// Overrides the pool's MaxDistance for this specific ruin.
    /// </summary>
    [DataField]
    public int? MaxDistance;

    [DataField]
    public bool PatchToPlanet = true;

    [DataField(required: true)]
    public int Priority = int.MinValue;

    /// <summary>
    /// List of components to grant to entities that enter the ruin.
    /// </summary>
    [DataField]
    public ComponentRegistry ComponentsToGrant = new();
}
