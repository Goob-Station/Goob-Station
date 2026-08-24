// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Mobs;

/// <summary>
/// Drops some loot when boss having this component dies.
/// </summary>
[RegisterComponent]
public sealed partial class SpawnLootOnDeathComponent : Component
{
    [DataField]
    public EntityTableSelector? Table;

    [DataField]
    public EntityTableSelector? SpecialTable;

    /// <summary>
    /// Whitelist for a weapon that is always checked when hitting the target.
    /// If target was damaged by something that doesn't pass this whitelist,
    /// the mob doesn't drop special loot and fallbacks to normal loot instead.
    /// </summary>
    [DataField("weaponWhitelist")]
    public EntityWhitelist? SpecialWeaponWhitelist;

    [DataField]
    public bool DeleteOnDeath;

    /// <summary>
    /// If true and the mob was killed with special weapon,
    /// and both loots are not null, drops both loots at once.
    /// </summary>
    [DataField]
    public bool DropBoth;

    /// <summary>
    /// Check if the boss got damaged by crusher only.
    /// True by default. Will immediately switch to false if anything else hit it. Even the environmental stuff.
    /// </summary>
    [ViewVariables]
    public bool DoSpecialLoot = true;
}
