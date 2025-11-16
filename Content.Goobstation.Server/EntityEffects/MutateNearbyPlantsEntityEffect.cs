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

        // should only work on plants in theory
        // but if it works on humans/dionas too then LOL let it be this way
        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Radius))
        {
            var mutargs = new PlantAdjustMutationLevel();
            mutargs.Effect(new(args.TargetEntity, entityManager));
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-mutate-plants");
}
