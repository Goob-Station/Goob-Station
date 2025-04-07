using Content.Server.Advertise;
using Content.Server.Advertise.Components;
using Content.Shared._DV.VendingMachines;

namespace Content.Server._DV.VendingMachines;

public sealed class ShopVendorSystem : SharedShopVendorSystem
{
    [Dependency] private readonly SpeakOnUIClosedSystem _speakOnUIClosed = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShopVendorComponent, TransformComponent>();
        var now = Timing.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            var ent = (uid, comp);
            var dirty = false;
            if (comp.Ejecting is {} ejecting && now > comp.NextEject)
            {
                Spawn(ejecting, xform.Coordinates);
                comp.Ejecting = null;
                dirty = true;
            }

            if (comp.Denying && now > comp.NextDeny)
            {
                comp.Denying = false;
                dirty = true;
            }

            if (dirty)
            {
                Dirty(uid, comp);
                UpdateVisuals(ent);
            }
        }
    }

    protected override void AfterPurchase(Entity<ShopVendorComponent> ent)
    {
        if (TryComp<SpeakOnUIClosedComponent>(ent, out var speak))
            _speakOnUIClosed.TrySetFlag((ent.Owner, speak));
    }
}
