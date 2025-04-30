// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural;

[Prototype("salvageDifficulty")]
public sealed partial class SalvageDifficultyPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// Color to be used in UI.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("color")]
    public Color Color = Color.White;

    /// <summary>
    /// How much loot this difficulty is allowed to spawn.
    /// </summary>
    [DataField("lootBudget", required : true)]
    public float LootBudget;

    /// <summary>
    /// How many mobs this difficulty is allowed to spawn.
    /// </summary>
    [DataField("mobBudget", required : true)]
    public float MobBudget;

    /// <summary>
    /// Budget allowed for mission modifiers like no light, etc.
    /// </summary>
    [DataField("modifierBudget")]
    public float ModifierBudget;

    [DataField("recommendedPlayers", required: true)]
    public int RecommendedPlayers;
}