using Content.Shared._DV.Pain;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Content.Server.Abilities.Psionics;
using Content.Server.Psionics;
using Content.Shared.Psionics.Glimmer;
using Content.Shared._DV.CosmicCult.Components;

namespace Content.Server._DV.EntityEffects.Effects;

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

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class CleanseCosmicCult : EntityEffectBase<CleanseCosmicCult>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-cleanse-cultist", ("chance", Probability));
}
