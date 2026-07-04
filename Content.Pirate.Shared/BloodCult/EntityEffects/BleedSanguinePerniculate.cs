// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.BloodCult.EntityEffects;

/// <summary>
/// Makes an entity bleed Sanguine Perniculate instead of their normal blood type while they metabolize Edge Essentia.
/// Server execution is handled via <see cref="EventEntityEffect{T}"/>.
/// </summary>
[UsedImplicitly, DataDefinition]
public sealed partial class BleedSanguinePerniculate : EventEntityEffect<BleedSanguinePerniculate>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-bleed-sanguine-perniculate", ("chance", Probability));
}
