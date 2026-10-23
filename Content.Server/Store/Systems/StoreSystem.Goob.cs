using Content.Server.GameTicking;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Store;
using Content.Shared.Store.Components;

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

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
}
