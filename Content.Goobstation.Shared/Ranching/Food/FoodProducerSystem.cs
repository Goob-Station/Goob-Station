using System.Linq;
using Content.Goobstation.Shared.Ranching.Happiness;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
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
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private EntityQuery<PreferencesHolderComponent> _preferencesQuery;
    private EntityQuery<HappinessPreferenceComponent> _happinessPreferenceQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _preferencesQuery = GetEntityQuery<PreferencesHolderComponent>();
        _happinessPreferenceQuery = GetEntityQuery<HappinessPreferenceComponent>();

        SubscribeLocalEvent<FoodProducerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<FoodProducerComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || ent.Comp.IsActive)
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
        // doafter here
        // audiostream here

        GrabFood(ent);
    }

    private void GrabFood(Entity<FoodProducerComponent> ent)
    {
        if (!_container.TryGetContainer(ent.Owner, ent.Comp.StorageContainer, out var foodContainer)
            || !_container.TryGetContainer(ent.Owner, ent.Comp.BeakerContainer, out var beakerContainer))
        {
            Log.Warning("Containers not found. Make sure the food producer has storagebase and beakerSlot");
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
        var beaker = beakerContainer.ContainedEntities.FirstOrNull();

        PrepareFeedSack(feedSack, foodContainer.ContainedEntities.ToList(), beaker);
    }

    private void PrepareFeedSack(EntityUid uid, List<EntityUid> food, EntityUid? beaker)
    {
        if (!_preferencesQuery.TryComp(uid, out var preferencesHolder) || !TryComp<FeedSackComponent>(uid, out var feedSackComponent))
            return;

        Color color = feedSackComponent.SeedColor;

        // first gather the preferences off the foods
        foreach (var foodEntity in food)
        {
            if (!_happinessPreferenceQuery.TryComp(foodEntity, out var happinessPreference))
                continue;

            preferencesHolder.Preferences.Add(happinessPreference.Preference);
        }

        // next from the reagent
        if (beaker is { } beakerEnt)
        {
            if (!_solutionContainer.TryGetSolution(beakerEnt, "beaker", out var solution))
                return;

            if (_appearance.TryGetData<Color>(beakerEnt, SolutionContainerVisuals.Color, out var beakerColor))
                color = beakerColor;

            foreach (var (reagentEnt, _) in solution.Value.Comp.Solution.GetReagentPrototypes(_proto))
            {
                if (reagentEnt.Preference == null)
                    continue;

                preferencesHolder.Preferences.Add(reagentEnt.Preference.Value);
            }
        }

        _appearance.SetData(uid, SeedColor.Color, color);
    }
}
