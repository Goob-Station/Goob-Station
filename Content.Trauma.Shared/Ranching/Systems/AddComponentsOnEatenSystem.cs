using Content.Shared.Nutrition;
using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed class AddComponentsOnEatenSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<AddComponentsOnEatenComponent, FullyEatenEvent>(OnEaten);
    }

    private void OnEaten(Entity<AddComponentsOnEatenComponent> ent, ref FullyEatenEvent args)
    {
        if (ent.Comp.WhiteList is null)
        {
            EntityManager.AddComponents(args.Eater, ent.Comp.Components);
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

        EntityManager.AddComponents(args.Eater, ent.Comp.Components);
    }
}
