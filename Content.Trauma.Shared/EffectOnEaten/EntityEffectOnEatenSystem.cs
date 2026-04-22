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
        if (ent.Comp.WhiteList is null)
        {
            _effects.ApplyEffects(args.Eater, ent.Comp.Effects, ent.Comp.Scale);
            return;
        }

        var isAllowed = false;
        var entityPrototype = MetaData(args.Eater).EntityPrototype;

        if (entityPrototype is null)
            return;

        foreach (var entity in ent.Comp.WhiteList)
        {
            if (entity.Id == entityPrototype.ID)
                isAllowed = true;
        }

        if (!isAllowed)
            return;

        _effects.ApplyEffects(args.Eater, ent.Comp.Effects, ent.Comp.Scale);
    }
}
