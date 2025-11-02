using System.Linq;
using Content.Goobstation.Shared.Ranching.Happiness;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Ranching.Food;

/// <summary>
/// This handles producing chicken feed sacks.
/// It spawns a chicken feed sack, which takes the colour of either the beaker's reagent OR defaults to orange.
/// </summary>
public sealed class FoodProducerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SolutionTransferSystem _solutionTransfer = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<PreferencesHolderComponent> _preferencesQuery;
    private EntityQuery<HappinessPreferenceComponent> _happinessPreferenceQuery;
    private EntityQuery<FeedSackComponent> _feedSackQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _preferencesQuery = GetEntityQuery<PreferencesHolderComponent>();
        _happinessPreferenceQuery = GetEntityQuery<HappinessPreferenceComponent>();
        _feedSackQuery = GetEntityQuery<FeedSackComponent>();

        SubscribeLocalEvent<FoodProducerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<FoodProducerComponent, FoodProducerDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<FoodProducerComponent, ContainerIsRemovingAttemptEvent>(StorageOpenAttempt);
        SubscribeLocalEvent<FoodProducerComponent, ItemSlotEjectAttemptEvent>(OnItemSlotEjectAttempt);
    }

    private void OnGetVerbs(Entity<FoodProducerComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || ent.Comp.IsActive)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("food-producer-verb"),
            IconEntity = GetNetEntity(ent.Owner),
            Act = () =>
            {
                ProduceFood(ent);
            }
        });
    }

    private void OnDoAfter(Entity<FoodProducerComponent> ent, ref FoodProducerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
        {
            ent.Comp.Audio = _audio.Stop(ent.Comp.Audio);
            return;
        }

        ent.Comp.Audio = _audio.Stop(ent.Comp.Audio);
        Dirty(ent);

        if (!_container.TryGetContainer(ent.Owner, ent.Comp.StorageContainer, out var foodContainer)
            || !_container.TryGetContainer(ent.Owner, ent.Comp.BeakerContainer, out var beakerContainer))
        {
            Log.Warning("Containers not found. Make sure the food producer has storagebase and beakerSlot");
            return;
        }

        MakeFood(ent, foodContainer, beakerContainer);
        args.Handled = true;
    }

    private void StorageOpenAttempt(Entity<FoodProducerComponent> ent, ref ContainerIsRemovingAttemptEvent args)
    {
        if (!ent.Comp.IsActive)
            return;

        _popup.PopupPredicted(Loc.GetString("food-producer-storage-fail"), ent.Owner, ent.Owner, PopupType.Medium);
        args.Cancel();
    }

    private void OnItemSlotEjectAttempt(Entity<FoodProducerComponent> ent, ref ItemSlotEjectAttemptEvent args)
    {
        if (!ent.Comp.IsActive || args.User == null)
            return;

        _popup.PopupClient(Loc.GetString("food-producer-storage-fail"), args.User.Value, args.User.Value, PopupType.Medium);
        args.Cancelled = true;
    }

    #region Helpers
    /// <summary>
    /// Starts the DoAfter if conditions pass.
    /// </summary>
    private void ProduceFood(Entity<FoodProducerComponent> ent)
    {
        // first, check if those containers exist in our entity
        if (!_container.TryGetContainer(ent.Owner, ent.Comp.StorageContainer, out var foodContainer)
            || !_container.TryGetContainer(ent.Owner, ent.Comp.BeakerContainer, out _))
        {
            Log.Warning("Containers not found. Make sure the food producer has storagebase and beakerSlot");
            return;
        }

        // check if the food container has more food than allowed, or no food at all
        if (!CanMakeFood(ent, foodContainer))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.Duration,
            new FoodProducerDoAfterEvent(),
            ent.Owner)
        {
            Hidden = true,
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        var stream = _audio.PlayPredicted(ent.Comp.GrindSound, ent.Owner, null);
        ent.Comp.Audio = stream?.Entity;

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
        {
            ent.Comp.Audio = _audio.Stop(ent.Comp.Audio);
            ent.Comp.IsActive = false;
            Dirty(ent);
            return;
        }

        ent.Comp.IsActive = true;
        Dirty(ent);
    }

    private void MakeFood(Entity<FoodProducerComponent> ent, BaseContainer foodContainer, BaseContainer beakerContainer)
    {
        if (_net.IsClient)
            return;

        var feedSack = SpawnAtPosition(ent.Comp.FeedSack, Transform(ent.Owner).Coordinates);

        var foodList = foodContainer.ContainedEntities.ToList();
        var beaker = beakerContainer.ContainedEntities.FirstOrNull();

        PrepareFeedSack(feedSack, foodList, beaker);

        var ev = new FoodProducedEvent(foodList.Count);
        RaiseLocalEvent(feedSack, ref ev);

        foreach (var food in foodList)
        {
            QueueDel(food);
        }

        ent.Comp.IsActive = false;
        Dirty(ent);
    }

    private void PrepareFeedSack(EntityUid uid, List<EntityUid> food, EntityUid? beaker)
    {
        if (!_preferencesQuery.TryComp(uid, out var preferencesHolder) || !_feedSackQuery.TryComp(uid, out var feedSackComponent))
            return;

        var color = feedSackComponent.SeedColor;
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
            if (!_solutionContainer.TryGetSolution(beakerEnt, "beaker", out var solution)
                || solution.Value.Comp.Solution.Volume <= 0)
                return;

            if (_appearance.TryGetData<Color>(beakerEnt, SolutionContainerVisuals.Color, out var beakerColor))
                color = beakerColor;

            foreach (var (reagentEnt, _) in solution.Value.Comp.Solution.GetReagentPrototypes(_proto))
            {
                if (reagentEnt.Preference == null)
                    continue;

                preferencesHolder.Preferences.Add(reagentEnt.Preference.Value);
            }

            // transfer the solution
            if (!_solutionContainer.TryGetSolution(uid, "sack", out var sackSolution) )
                return;

            _solutionTransfer.Transfer(uid,
                beakerEnt,
                solution.Value,
                uid,
                sackSolution.Value,
                solution.Value.Comp.Solution.Volume);
        }

        _appearance.SetData(uid, SeedColor.Color, color);
    }

    private bool CanMakeFood(Entity<FoodProducerComponent> ent, BaseContainer foodContainer)
    {
        if (foodContainer.Count > ent.Comp.MaxFood)
        {
            _popup.PopupPredicted(
                Loc.GetString("food-producer-max-food", ("maxFood", ent.Comp.MaxFood)),
                ent.Owner,
                ent.Owner,
                PopupType.Medium);

            return false;
        }

        if (foodContainer.Count <= 0)
        {
            _popup.PopupPredicted(Loc.GetString("food-producer-no-food"), ent.Owner, ent.Owner, PopupType.Medium);
            return false;
        }

        return true;
    }
    #endregion
}
