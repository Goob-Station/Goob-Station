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
        EntityManager.AddComponents(args.User, ent.Comp.Components);
    }
}
