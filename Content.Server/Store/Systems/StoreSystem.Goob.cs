using Content.Goobstation.Maths.FixedPoint;
using Content.Server.GameTicking;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    private const string TreeInterface = "StoreTreeBoundUserInterface";

    private void InitializeGoob()
    {
        SubscribeLocalEvent<StoreComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnPolymorphed(Entity<StoreComponent> ent, ref PolymorphedEvent args)
    {
        if (args.IsRevert)
            return;

        _polymorph.CopyPolymorphComponent<StoreComponent>(ent, args.NewEntity);
    }

    private void OnPurchase(ListingData listing)
    {
        if (!_proto.TryIndex<ListingPrototype>(listing.ID, out var prototype))
            return;

        // updating restocktime
        var now = _timing.CurTime.Subtract(_ticker.RoundStartTimeSpan);
        if (prototype.ResetRestockOnPurchase)
        {
            var restockDuration = prototype.RestockTime;
            listing.RestockTime = now + restockDuration;
        }
        if (listing.ResetRestockOnPurchase)
        {
            var restockDuration = listing.RestockAfterPurchase ?? listing.RestockTime;
            listing.RestockTime = now + restockDuration;
        }
    }

    private void EnsureTreeInterface(Entity<StoreComponent> ent)
    {
        if (ent.Comp.Tree == null)
            return;

        if (!_ui.TryGetInterfaceData(ent.Owner, StoreUiKey.Key, out var data) || data.ClientType == TreeInterface)
            return;

        _ui.SetUi(ent.Owner, StoreUiKey.Key, new InterfaceData(TreeInterface, data.InteractionRange, data.RequireInputValidation));
    }

    private bool TrySetTreeUiState(Entity<StoreComponent> ent, Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> balance)
    {
        if (ent.Comp.Tree is not { } treeId || !_proto.TryIndex(treeId, out var tree))
            return false;

        var listings = new HashSet<ListingDataWithCostModifiers>();
        foreach (var listing in ent.Comp.LastAvailableListings)
        {
            if (tree.Nodes.ContainsKey(listing.ID))
                listings.Add(listing);
        }

        var purchased = new HashSet<ProtoId<ListingPrototype>>();
        foreach (var listing in ent.Comp.FullListingsCatalog)
        {
            if (listing.PurchaseAmount > 0 && tree.Nodes.ContainsKey(listing.ID))
                purchased.Add(listing.ID);
        }

        _ui.SetUiState(ent.Owner, StoreUiKey.Key, new StoreTreeUpdateState(treeId, listings, purchased, balance));
        return true;
    }
}
