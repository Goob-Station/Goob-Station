using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Ranching.Food;

/// <summary>
/// This handles producing procedural food.
/// </summary>
public sealed class FoodProducerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoodProducerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<FoodProducerComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("food-producer-verb"),
            IconEntity = GetNetEntity(ent.Owner),
            Act = () =>
            {
                ProduceFood(ent);
            }
        });
    }

    private void ProduceFood(Entity<FoodProducerComponent> ent)
    {
        GrabFood(ent);
    }

    private void GrabFood(Entity<FoodProducerComponent> ent)
    {
        if (!_container.TryGetContainer(ent.Owner, ent.Comp.StorageContainer, out var foodContainer)
            || !_container.TryGetContainer(ent.Owner, ent.Comp.BeakerContainer, out var beakerContainer))
        {
            Log.Info("Containers not found");
            return;
        }

        if (foodContainer.Count > ent.Comp.MaxFood)
        {
            _popup.PopupPredicted(
                Loc.GetString("food-producer-max-food", ("maxFood", ent.Comp.MaxFood)),
                ent.Owner,
                ent.Owner,
                PopupType.Medium);

            return;
        }



    }

    private void OutputFood(Entity<FoodProducerComponent> ent)
    {

    }
}
