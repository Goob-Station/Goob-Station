using Content.Shared.EntityEffects;
using Content.Shared.Nutrition;

namespace Content.Trauma.Shared.EffectOnEaten;

public sealed class EntityEffectOnEatenSystem : EntitySystem
{

    [Dependency] private readonly SharedEntityEffectsSystem _effects = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EntityEffectOnEatenComponent, FullyEatenEvent>(OnEaten);
    }

    private void OnEaten(Entity<EntityEffectOnEatenComponent> ent, ref FullyEatenEvent args)
    {
        _effects.ApplyEffects(args.Eater, ent.Comp.Effects, ent.Comp.Scale);
    }
}
