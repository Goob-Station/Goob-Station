using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class SummonPortalSystem : EntitySystem
{

    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private static readonly EntProtoId VoidPortal = "VoidPortal";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonPortalComponent, SummonPortalEvent>(OnSummonPortal);
    }

    public void OnSummonPortal(Entity<SummonPortalComponent> ent, ref SummonPortalEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        //TO DO: Add logic for asking if the player wants to move the portal's location, rather than just denying a new portal.
        if (comp.CurrentActivePortals >= comp.PortalLimit)
        {
            _popup.PopupPredicted(Loc.GetString("wraith-portal-limit"), uid, uid, PopupType.LargeCaution);
            return;
        }

        var xform = Transform(uid);

        // Portal can only be summoned while on grid
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-portal-anchor"), uid, uid);
            return;
        }

        _popup.PopupPredicted(Loc.GetString("wraith-portal-success"), uid, uid, PopupType.Large);
        var voidUid = Spawn(VoidPortal, _transform.GetMapCoordinates(uid, xform: xform));
        comp.CurrentActivePortals++;
        args.Handled = true;

        //TO DO: Add logic for if the portal gets destroyed. (Though honestly that might end up just being handled through the 
    }
}
