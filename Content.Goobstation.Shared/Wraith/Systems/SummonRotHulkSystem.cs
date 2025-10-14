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
            _popup.PopupEntity(Loc.GetString("wraith-rot-hulk-rise"), ent.Owner, ent.Owner);
            return;
        }

        //TO DO: Would be cool if the trash slowly was dragged into a center point and only then spawn the rot hulk in the middle of that, rather than just deleting. For parity's sake and all.
        //Leaving this for part 2, it's just cosmetic.
        if (_net.IsServer) //Only server can delete networked entities.
        {
            foreach (var trash in nearbyTrash)
                QueueDel(trash);
        }

        // Choose which prototype to spawn
        var isBuff = nearbyTrash.Count >= comp.BuffThreshold;
        var proto = isBuff
            ? comp.BuffRotHulkProto
            : comp.RotHulkProto;

        var hulk = PredictedSpawnAtPosition(proto, xform.Coordinates);

        // Show the localized message, substituting the spawned entity name
        _popup.PopupPredicted(Loc.GetString("wraith-rot-hulk-emerge", ("E", hulk)), hulk, ent.Owner);

        args.Handled = true;
    }

}
