// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Shared.Tag;
using Content.Trauma.Common.Heretic;
using Content.Trauma.Common.Nutrition;
using Content.Trauma.Shared.Ranching.Components;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed class RanchingFoodSystem : EntitySystem
{
    [Dependency] private readonly SharedInternalResourcesSystem _internalResources = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MostRecentlyEatenFoodTagsComponent, FullyAteEvent>(OnFoodEaten);
        SubscribeLocalEvent<FavoriteFoodComponent, ConsumingFoodEvent>(OnFavoriteEaten);
    }

    private void OnFavoriteEaten(Entity<FavoriteFoodComponent> ent, ref ConsumingFoodEvent args)
    {
        if (!TryComp<TagComponent>(args.Food, out var tag) || !TryComp<HappinessComponent>(ent.Owner, out var happiness))
            return;

        foreach (var food in tag.Tags)
        {
            if (ent.Comp.Tag.Contains(food))
                AddHappiness((ent.Owner, happiness), ent.Comp.Amount);

        }
    }

    private void OnFoodEaten(Entity<MostRecentlyEatenFoodTagsComponent> ent, ref FullyAteEvent args)
    {
        if (!TryComp<TagComponent>(args.Food, out var tag))
            return;

        foreach (var food in tag.Tags)
        {
            ent.Comp.Tag.Add(food);
        }
    }

    public void AddHappiness(Entity<HappinessComponent> ent, int amount)
    {
        if (!TryComp<InternalResourcesComponent>(ent, out var internalResources))
            return;

        var happinessResource = _prototype.Index(ent.Comp.HappinessResource);

        foreach (var type in internalResources.CurrentInternalResources)
        {
            if (type.InternalResourcesType == happinessResource)
                _internalResources.TryUpdateResourcesAmount(ent.Owner, type, amount);
        }
    }
}
