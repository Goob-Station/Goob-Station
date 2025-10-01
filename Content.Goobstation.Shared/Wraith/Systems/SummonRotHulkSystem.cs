using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class SummonRotHulkSystem : EntitySystem
{

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly INetManager _net = default!;



    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonRotHulkComponent, SummonRotHulkEvent>(OnSummonHulk);
    }

    public void OnSummonHulk(Entity<SummonRotHulkComponent> ent, ref SummonRotHulkEvent args)
    {
        if (args.Handled)
            return;

        var uid = ent.Owner;
        var comp = ent.Comp;
        var xform = Transform(uid);

        var nearbyTrash = new List<EntityUid>();
        foreach (var e in _lookup.GetEntitiesInRange(xform.Coordinates, comp.SearchRadius))
        {
            if (_tags.HasTag(e, comp.TrashTag))
                nearbyTrash.Add(e);
            if (nearbyTrash.Count >= comp.MaxTrash)
                break;
        }

        if (nearbyTrash.Count < comp.MinTrash)
        {
            _popup.PopupPredicted("This place is much too clean to summon a rot hulk.", uid, uid);
            return;
        }

        if (_net.IsServer) //Only server can delete networked entities.
        {
            foreach (var trash in nearbyTrash)
                QueueDel(trash);
        }

        var proto = nearbyTrash.Count >= comp.BuffThreshold
            ? comp.BuffRotHulkProto
            : comp.RotHulkProto;

        var hulk = PredictedSpawnAtPosition(proto, xform.Coordinates);
        _popup.PopupPredicted("The filth coalesces into a grotesque servant!", uid, uid);

        args.Handled = true;
    }

}
