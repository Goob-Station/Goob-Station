// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.EntitySystems;
using Robust.Shared.Prototypes;
using Content.Shared.EntityConditions;
using Content.Shared.Atmos.Components;

namespace Content.Server.EntityEffects.EffectConditions;

// I Will in fact kill someone if i have to put this under TransformComponent
public sealed partial class PressureThresholdSystem : EntityConditionSystem<MovedByPressureComponent, PressureThreshold>
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    protected override void Condition(Entity<MovedByPressureComponent> entity, ref EntityConditionEvent<PressureThreshold> args)
    {
        var effect = args.Condition;

        if (!TryComp<TransformComponent>(entity.Owner, out var transform))
        {
            args.Result = false;
            return;
        }

        if (effect.WorksOnLavaland && HasComp<LavalandMapComponent>(transform.MapUid))
        {
            args.Result = true;
            return;
        }

        var mix = _atmos.GetTileMixture((entity.Owner, transform));
        var pressure = mix?.Pressure ?? 0f;

        args.Result = pressure >= effect.Min && pressure <= effect.Max;
    }
}

public sealed partial class PressureThreshold : EntityConditionBase<PressureThreshold>
{
    [DataField]
    public bool WorksOnLavaland = false;

    [DataField]
    public float Min = float.MinValue;

    [DataField]
    public float Max = float.MaxValue;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-pressure-threshold",
            ("min", Min),
            ("max", Max));
    }
}
