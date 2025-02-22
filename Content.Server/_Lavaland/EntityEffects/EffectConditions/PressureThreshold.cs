using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Content.Server.Atmos.EntitySystems;
using Content.Server._Lavaland.Procedural.Components;

namespace Content.Server.EntityEffects.EffectConditions;

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
        if (!args.EntityManager.TryGetComponent<TransformComponent>(args.TargetEntity, out var transform))
            return false;

        if (WorksOnLavaland && args.EntityManager.HasComponent<LavalandMapComponent>(transform.MapUid))
            return true;

        var mix = args.EntityManager.System<AtmosphereSystem>().GetTileMixture((args.TargetEntity, transform));
        var pressure = mix?.Pressure ?? 0f;
        return pressure >= Min && pressure <= Max;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-pressure-threshold",
            ("min", Min),
            ("max", Max));
    }
}
