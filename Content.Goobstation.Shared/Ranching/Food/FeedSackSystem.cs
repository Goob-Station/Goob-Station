using Content.Goobstation.Shared.Ranching.Happiness;
using Content.Shared.Charges.Systems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Ranching.Food;


public abstract class SharedFeedSackSystem : EntitySystem
{
    [Dependency] private readonly TurfSystem _turfSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly HappinessContainerSystem _happiness = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FeedSackComponent, AfterInteractEvent>(OnAfterInteract);
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

        foreach (var pref in _happiness.GetPreferences(ent.Owner))
        {
            preferences.Preferences.Add(pref);
        }

        ChangeFeedColour(ent.Comp.SeedColor, chickenFeed);
    }

    protected virtual void ChangeFeedColour(Color color, EntityUid feedUid) {}
}
