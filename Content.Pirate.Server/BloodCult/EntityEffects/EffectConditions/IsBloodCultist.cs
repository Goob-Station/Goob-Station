// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Shared.BloodCult;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntityEffects.EffectConditions;

/// <summary>
/// Condition that checks if an entity is a Blood Cultist.
/// Used for effects that should only affect cultists (like holy smoke).
/// </summary>
[UsedImplicitly]
public sealed partial class IsBloodCultist : EntityEffectCondition
{
    /// <summary>
    /// </summary>
    [DataField]
    public bool Invert = false;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        // Verify target entity exists (helps avoid errors if this entity is deleted in the same tick)
        if (!args.EntityManager.EntityExists(args.TargetEntity))
            return false;

        var hasCultComponent = args.EntityManager.HasComponent<BloodCultistComponent>(args.TargetEntity);
        return hasCultComponent ^ Invert;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-is-blood-cultist", ("invert", Invert));
    }
}

