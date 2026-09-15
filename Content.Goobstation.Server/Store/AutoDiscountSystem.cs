using System.Linq;
using Content.Server.Store.Systems;
using Content.Server.StoreDiscount.Systems;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Server.Store;

public sealed class AutoDiscountSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoDiscountComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<AutoDiscountComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out StoreComponent? store))
            return;

        _store.RefreshAllListings(store);

        var ev = new StoreInitializedEvent(EntityUid.Invalid, ent, true, store.FullListingsCatalog.ToList());
        RaiseLocalEvent(ref ev);
    }
}
