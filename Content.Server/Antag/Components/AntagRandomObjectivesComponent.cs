// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server.Antag.Components;

/// <summary>
/// Gives antags selected by this rule a random list of objectives.
/// </summary>
[RegisterComponent, Access(typeof(AntagRandomObjectivesSystem))]
public sealed partial class AntagRandomObjectivesComponent : Component
{
    /// <summary>
    /// Each set of objectives to add.
    /// </summary>
    [DataField(required: true)]
    public List<AntagObjectiveSet> Sets = new();

    /// <summary>
    /// If the total difficulty of the currently given objectives exceeds, no more will be given.
    /// </summary>
    [DataField(required: true)]
    public float MaxDifficulty;
}

/// <summary>
/// A set of objectives to try picking.
/// Difficulty is checked over all sets, but each set has its own probability and pick count.
/// </summary>
[DataRecord]
public partial record struct AntagObjectiveSet()
{
    /// <summary>
    /// The grouping used by the objective system to pick random objectives.
    /// First a group is picked from these, then an objective from that group.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> Groups = string.Empty;

    /// <summary>
    /// Probability of this set being used.
    /// </summary>
    [DataField]
    public float Prob = 1f;

    /// <summary>
    /// Number of times to try picking objectives from this set.
    /// Even if there is enough difficulty remaining, no more will be given after this.
    /// </summary>
    [DataField]
    public int MaxPicks = 20;
}