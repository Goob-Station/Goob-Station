using System.Linq;
using Content.Goobstation.Common.Ranching;
using Content.Goobstation.Shared.Ranching.Happiness;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Ranching.Food;

/// <summary>
/// This handles producing procedural food.
/// </summary>
public sealed class SharedFoodProducerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

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

        if (foodContainer.Count <= 0)
        {
            _popup.PopupPredicted(Loc.GetString("food-producer-no-food"), ent.Owner, ent.Owner, PopupType.Medium);
            return;
        }

        if (_net.IsClient)
            return;

        var feedSack = SpawnAtPosition(ent.Comp.FeedSack, Transform(ent.Owner).Coordinates);

        PrepareFeedSack(feedSack, foodContainer.ContainedEntities.ToList(), beakerContainer.ContainedEntities.FirstOrNull());
    }

    private void PrepareFeedSack(EntityUid uid, List<EntityUid> food, EntityUid? reagent)
    {
        if (!TryComp<PreferencesHolderComponent>(uid, out var preferencesHolder))
            return;

        // first gather the preferences off the foods
        foreach (var foodEntity in food)
        {
            if (!TryComp<HappinessPreferenceComponent>(foodEntity, out var happinessPreference))
                continue;

            preferencesHolder.Preferences.Add(happinessPreference.Preference);
        }

        // next from the reagent
        if (reagent is { } reagentEnt)
        {
            if (!_solutionContainer.TryGetSolution(reagentEnt, "beaker", out var solution))
                return;

            foreach (var (rEnt, _) in solution.Value.Comp.Solution.GetReagentPrototypes(_proto))
            {
                if (rEnt.Preference == null)
                    continue;

                preferencesHolder.Preferences.Add(rEnt.Preference.Value);
            }
        }
    }
}
