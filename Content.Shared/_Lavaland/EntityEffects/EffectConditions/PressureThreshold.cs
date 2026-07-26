// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Atmos;
using Content.Shared._Lavaland.Procedural.Components;
using Content.Shared.EntityEffects;
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

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<TransformComponent>(args.TargetEntity, out var transform))
            return false;

        if (WorksOnLavaland && args.EntityManager.HasComponent<LavalandMapComponent>(transform.MapUid))
            return true;

        // TODO this is a terrible workaround and it's fixable only by making atmos partially predicted AAAAAAAAAAAAAAA
        var mix = args.EntityManager.System<SharedLavalandAtmosphereSystem>().GetTileMixture((args.TargetEntity, transform));
        if (mix == null)
            return false;

        var pressure = mix?.Pressure;
        return pressure >= Min && pressure <= Max;
    }

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-pressure-threshold",
            ("min", Min),
            ("max", Max));
    }
}
