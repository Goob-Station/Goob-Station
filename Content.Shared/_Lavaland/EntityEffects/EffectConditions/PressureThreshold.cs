// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.EntityConditions;
using Content.Shared.EntityConditions.Conditions;

namespace Content.Shared._Lavaland.EntityEffects.EffectConditions;

public sealed partial class PressureThresholdEntityConditionSystem : EntityConditionSystem<TransformComponent, PressureThresholdCondition>
{
    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<PressureThresholdCondition> args)
    {
        args.Result = false;
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class PressureThresholdCondition : EntityConditionBase<PressureThresholdCondition>
{
    [DataField]
    public bool WorksOnLavaland = false;

    [DataField]
    public float Min = float.MinValue;

    [DataField]
    public float Max = float.MaxValue;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-pressure-threshold",
            ("min", Min),
            ("max", Max));
    }
}
