// SPDX-FileCopyrightText: 2024 drakewill-CRL <46307022+drakewill-CRL@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Serialization;

namespace Content.Shared.Random;

/// <summary>
///     Data that specifies the odds and effects of possible random plant mutations.
/// </summary>
[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class RandomPlantMutation
{
    /// <summary>
    ///     Odds of this mutation occurring with 1 point of mutation severity on a plant.
    /// </summary>
    [DataField]
    public float BaseOdds = 0;

    /// <summary>
    ///     The name of this mutation.
    /// </summary>
    [DataField]
    public string Name = "";

    /// <summary>
    /// The actual EntityEffect to apply to the target
    /// </summary>
    [DataField]
    public EntityEffect Effect = default!;

    /// <summary>
    /// This mutation will target the harvested produce
    /// </summary>
    [DataField]
    public bool AppliesToProduce = true;

    /// <summary>
    /// This mutation will target the growing plant as soon as this mutation is applied.
    /// </summary>
    [DataField]
    public bool AppliesToPlant = true;

    /// <summary>
    /// This mutation stays on the plant and its produce. If false while AppliesToPlant is true, the effect will run when triggered.
    /// </summary>
    [DataField]
    public bool Persists = true;
}