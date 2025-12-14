using Content.Server.EntityEffects.Effects.PlantMetabolism;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class MutateNearbyPlantsEntityEffect : EntityEffect
{
    [DataField] public float Radius = 5f;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var lookupSys = entityManager.System<EntityLookupSystem>();
        var entityEffects = entityManager.System<EntityEffectSystem>();

        // should only work on plants in theorem
        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Radius))
            entityEffects.Effect(new PlantAdjustMutationLevel(), new(entity, entityManager));
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-mutate-plants");
}
