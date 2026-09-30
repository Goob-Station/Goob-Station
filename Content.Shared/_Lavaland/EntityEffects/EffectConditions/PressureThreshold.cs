// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Atmos;
using Content.Shared._Lavaland.Procedural.Components;
using Robust.Shared.Prototypes;
using Content.Shared.EntityConditions;

namespace Content.Shared._Lavaland.EntityEffects.EffectConditions;

public sealed partial class PressureThresholdEntityConditionSystem : EntityConditionSystem<TransformComponent, PressureThresholdCondition>
{
    [Dependency] private readonly SharedLavalandAtmosphereSystem _atmosLavaland = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<PressureThresholdCondition> args)
    {
        var transform = entity.Comp;

        if (args.Condition.WorksOnLavaland && HasComp<LavalandMapComponent>(transform.MapUid))
        {
            args.Result = true;
            return;
        }

        // TODO this is a terrible workaround and it's fixable only by making atmos partially predicted AAAAAAAAAAAAAAA
        var mix = _atmosLavaland.GetTileMixture(entity.AsNullable());
        if (mix == null)
        {
            args.Result = false;
            return;
        }

        var pressure = mix.Pressure;
        args.Result = pressure >= args.Condition.Min && pressure <= args.Condition.Max;
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class PressureThresholdCondition : EntityConditionBase<PressureThresholdCondition>
{
    [DataField]
    public bool WorksOnLavaland;

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
