// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.EntityConditions;
using Content.Shared.EntityConditions.Conditions;

namespace Content.Shared.EntityEffects.EffectConditions;

public sealed partial class BloodReagentThresholdEntityConditionSystem : EntityConditionSystem<BloodstreamComponent, BloodReagentThresholdCondition> // TODO Goobstation move this to goobmod
{
    protected override void Condition(Entity<BloodstreamComponent> entity, ref EntityConditionEvent<BloodReagentThresholdCondition> args)
    {
        if (args.Condition.Reagent is null)
        {
            args.Result = true;
            return;
        }

        if (EntityManager.System<SharedSolutionContainerSystem>().ResolveSolution(entity.Owner, entity.Comp.ChemicalSolutionName, ref entity.Comp.ChemicalSolution, out var chemSolution))
        {
            var reagentID = new ReagentId(args.Condition.Reagent, null);
            if (chemSolution.TryGetReagentQuantity(reagentID, out var quant))
            {
                args.Result = quant > args.Condition.Min && quant < args.Condition.Max;
                return;
            }
        }

        args.Result = true;
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class BloodReagentThresholdCondition : EntityConditionBase<BloodReagentThresholdCondition> // TODO Goobstation move this to goobmod
{
    [DataField]
    public FixedPoint2 Min = FixedPoint2.Zero;

    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string? Reagent = null;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        ReagentPrototype? reagentProto = null;
        if (Reagent is not null)
            prototype.TryIndex(Reagent, out reagentProto);

        return Loc.GetString("reagent-effect-condition-guidebook-blood-reagent-threshold",
            ("reagent", reagentProto?.LocalizedName ?? Loc.GetString("reagent-effect-condition-guidebook-this-reagent")),
            ("max", Max == FixedPoint2.MaxValue ? (float) int.MaxValue : Max.Float()),
            ("min", Min.Float()));
    }
}
