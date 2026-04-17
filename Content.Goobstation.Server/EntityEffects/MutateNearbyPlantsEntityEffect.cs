using Content.Server.Botany.Components;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects.Botany.PlantAttributes;
using Robust.Shared.Prototypes;

//todo put this back in shared i just cant be bothered atm
namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class MutateNearbyPlantsEntityEffectSystem : EntityEffectSystem<PlantHolderComponent, MutateNearbyPlantsEntityEffect>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedEntityEffectsSystem _effects = default!;

    protected override void Effect(Entity<PlantHolderComponent> entity, ref EntityEffectEvent<MutateNearbyPlantsEntityEffect> args)
    {
        foreach (var target in _lookup.GetEntitiesInRange(entity.Owner, args.Effect.Radius))
        {
            _effects.TryApplyEffect(target, new PlantAdjustMutationLevel(), args.Scale);
        }
    }
}

public sealed partial class MutateNearbyPlantsEntityEffect : EntityEffectBase<MutateNearbyPlantsEntityEffect>
{
    [DataField] public float Radius = 5f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-mutate-plants");
}
