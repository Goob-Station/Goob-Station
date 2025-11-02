using Content.Goobstation.Shared.Ranching.Happiness;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Ranching.Food;

/// <summary>
/// Handles spawning food for the chicken.
/// It transfers the preferences to the chicken feed.
/// It allows you to place a set amount of chicken feed depending on how much food you put in the Food Producer.
/// </summary>
public sealed class FeedSackSystem : EntitySystem
{
    // todo: visuals for when it gets empty
    [Dependency] private readonly TurfSystem _turfSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly HappinessContainerSystem _happiness = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SolutionTransferSystem _solutionTransfer = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FeedSackComponent, AfterInteractEvent>(OnAfterInteract);

        SubscribeLocalEvent<LimitedChargesComponent, FoodProducedEvent>(OnFoodProduced);
    }

    private void OnAfterInteract(Entity<FeedSackComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        var gridUid = _transform.GetGrid(args.ClickLocation);
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return;
        if (!_maps.TryGetTileRef(gridUid.Value, grid, args.ClickLocation, out var tileRef))
            return;

        bool IsTileClear()
        {
            return tileRef.Tile.IsEmpty == false && !_turfSystem.IsTileBlocked(tileRef, CollisionGroup.MobMask);
        }

        if (!IsTileClear() || ent.Comp.Deleted)
            return;

        if (!_charges.TryUseCharge(ent.Owner))
            return;

        if (_net.IsClient)
            return;

        var chickenFeed = SpawnAtPosition(ent.Comp.ChickenFeed, args.ClickLocation);

        if (!TryComp<PreferencesHolderComponent>(chickenFeed, out var preferences))
        {
            Log.Warning("Chicken food must always have PreferencesHolderComponent");
            QueueDel(chickenFeed);
            return;
        }

        // transfer preferences to chicken feed
        foreach (var pref in _happiness.GetPreferences(ent.Owner))
        {
            preferences.Preferences.Add(pref);
        }

        TransferSolutions(args.User, ent.Owner, chickenFeed, _charges.GetCurrentCharges(ent.Owner));

        if (!_appearance.TryGetData<Color>(ent.Owner, SeedColor.Color, out var color))
            return;

        _appearance.SetData(chickenFeed, SeedColor.Color, color);
    }

    private void OnFoodProduced(Entity<LimitedChargesComponent> ent, ref FoodProducedEvent args) =>
        _charges.SetCharges(ent.Owner, ent.Comp.LastCharges * args.FoodAmount);

    #region Helper
    private void TransferSolutions(EntityUid user, EntityUid sack, EntityUid food, int chargesLeft)
    {
        if (!_solutionContainer.TryGetSolution(sack, "sack", out var sackSolution)
            || !_solutionContainer.TryGetSolution(food, "seed", out var seedSolution))
            return;

        if (sackSolution.Value.Comp.Solution.Volume <= 0)
            return;

        _solutionTransfer.Transfer(
            user,
            sack,
            sackSolution.Value,
            food,
            seedSolution.Value,
            sackSolution.Value.Comp.Solution.Volume / chargesLeft);
    }
    #endregion
}
