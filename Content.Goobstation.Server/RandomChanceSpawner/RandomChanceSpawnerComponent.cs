// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.RandomChanceSpawner;

/// <summary>
/// Shitcode my beloved 
/// </summary>
[RegisterComponent, EntityCategory("Spawner")]
public sealed partial class RandomChanceSpawnerComponent : Component
{
    [DataField]
    public Dictionary<EntProtoId, float> ToSpawn = new();

    /// <summary>
    /// If true, the spawner will put the item the hand of whoever holds it.
    /// (e.g. when used in a starting gears inhand list).
    /// The spawner needs an Item component for this.
    /// </summary>
    [DataField]
    public bool PutInHand;

    [ViewVariables]
    public List<EntityUid> Spawned = new();
}
