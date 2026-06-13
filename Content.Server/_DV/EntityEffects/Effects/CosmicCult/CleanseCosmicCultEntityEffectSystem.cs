using Content.Shared._DV.EntityEffects.Effects.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.EntityEffects;

namespace Content.Server._DV.EntityEffects.Effects.CosmicCult;

/// <summary>
///     Supresses pain based on how much of the pain suppressing reagent is in the system.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class CleanseCosmicCultEntityEffectSystem : EntityEffectSystem<TransformComponent, CleanseCosmicCult>
{
    [Dependency] private readonly EntityManager _ent = default!;
    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<CleanseCosmicCult> args)
    {
        if (_ent.HasComponent<CosmicCultComponent>(entity))
            _ent.EnsureComponent<CleanseCultComponent>(entity); // We just slap them with the component and let the Deconversion system handle the rest.
    }
}
