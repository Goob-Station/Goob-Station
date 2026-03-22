using Content.Shared.Tag;
using Content.Trauma.Common.Nutrition;
using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Systems;

/// <summary>
/// This handles storing the tag of the most recently eaten food, used for ranching
/// </summary>
public sealed class MostRecentlyEatenFoodTagSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MostRecentlyEatenFoodTagsComponent, FullyAteEvent>(OnFoodEaten);
    }

    private void OnFoodEaten(Entity<MostRecentlyEatenFoodTagsComponent> ent, ref FullyAteEvent args)
    {
        if (!TryComp<TagComponent>(args.Food, out var tag))
            return;

        ent.Comp.Tag = tag.Tags;
    }
}
