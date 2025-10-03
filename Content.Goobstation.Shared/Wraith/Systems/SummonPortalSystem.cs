using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;
using Content.Shared.Physics;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class SummonPortalSystem : EntitySystem
{

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonPortalComponent, SummonPortalEvent>(OnSummonPortal);
    }

    public void OnSummonPortal(Entity<SummonPortalComponent> ent, ref SummonPortalEvent args)
    {
        if (_physics.GetEntitiesIntersectingBody(ent.Owner, (int) CollisionGroup.Impassable).Count > 0)
        {
            _popup.PopupPredicted(Loc.GetString("wraith-in-solid"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        if (ent.Comp.CurrentActivePortals >= ent.Comp.PortalLimit)
        {
            _popup.PopupPredicted(Loc.GetString("wraith-portal-limit"), ent.Owner, ent.Owner, PopupType.LargeCaution);
            return;
        }

        var xform = Transform(ent.Owner);
        // Portal can only be summoned while on grid
        if (!HasComp<MapGridComponent>(xform.GridUid))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-portal-anchor"), ent.Owner, ent.Owner);
            return;
        }

        if (_netManager.IsServer)
        {
            var portal = Spawn(ent.Comp.VoidPortal, xform.Coordinates);
            _popup.PopupEntity(Loc.GetString("wraith-portal-success"), ent.Owner, ent.Owner, PopupType.Large);

            if (TryComp<VoidPortalComponent>(portal, out var voidPortal))
                voidPortal.PortalOwner = ent.Owner;
        }

        ent.Comp.CurrentActivePortals++;
        Dirty(ent);

        args.Handled = true;
    }

    public void PortalDestroyed(Entity<SummonPortalComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.CurrentActivePortals--;
    }
}
