// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.Loot;

/// <summary>
/// Spawned inside of a salvage mission.
/// </summary>
[Prototype("salvageLoot")]
public sealed partial class SalvageLootPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// Should this loot always spawn if possible. Used for stuff such as ore.
    /// </summary>
    [DataField("guaranteed")] public bool Guaranteed;

    /// <summary>
    /// All of the loot rules
    /// </summary>
    [DataField("loots")]
    public List<IDungeonLoot> LootRules = new();
}