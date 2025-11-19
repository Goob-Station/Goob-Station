using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Server.Wraith.Systems;

public sealed partial class SummonPortalSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SummonPortalComponent, SummonPortalEvent>(OnSummonPortal);
    }
    public void OnSummonPortal(Entity<SummonPortalComponent> ent, ref SummonPortalEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var xform = Transform(uid);
        var grid = _transform.GetGrid(ent.Owner);
        var center = xform.Coordinates;

        if (_physics.GetEntitiesIntersectingBody(ent.Owner, (int) CollisionGroup.Impassable).Count > 0)
        {
            _popup.PopupPredicted(Loc.GetString("wraith-portal-blocked"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        if (!HasComp<MapGridComponent>(xform.GridUid))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-portal-cannot-open"), ent.Owner, ent.Owner);
            return;
        }

        if (!_netManager.IsServer)
            return;

        // Delete old portal if exists
        if (ent.Comp.CurrentPortal != null)
        {
            QueueDel(ent.Comp.CurrentPortal.Value);
            ent.Comp.CurrentPortal = null;
            ent.Comp.CurrentActivePortals = 0;
        }

        // Spawn the ritual circle
        var portal = Spawn(ent.Comp.RitualCircle, center);

        ent.Comp.CurrentPortal = portal;
        ent.Comp.CurrentActivePortals = 1;
        Dirty(ent);

        _popup.PopupEntity(Loc.GetString("wraith-portal-gathering"), ent.Owner, ent.Owner, PopupType.Small);

        args.Handled = true;
    }

    public void PortalDestroyed(Entity<SummonPortalComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.CurrentPortal = null;
        ent.Comp.CurrentActivePortals = 0;
        Dirty(ent);
    }
}
