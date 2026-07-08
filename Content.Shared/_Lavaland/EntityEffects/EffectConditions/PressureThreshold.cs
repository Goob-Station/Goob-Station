// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Content.Shared.Atmos.EntitySystems;

namespace Content.Shared.EntityEffects.EffectConditions;

public sealed partial class PressureThreshold : EntityEffectCondition
{
    [DataField]
    public bool WorksOnLavaland = false;

    [DataField]
    public float Min = float.MinValue;

    [DataField]
    public float Max = float.MaxValue;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        return false;
    }
    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-pressure-threshold",
            ("min", Min),
            ("max", Max));
    }

}
