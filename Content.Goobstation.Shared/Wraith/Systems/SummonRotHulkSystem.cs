using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class SummonRotHulkSystem : EntitySystem
{

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonRotHulkComponent, SummonRotHulkEvent>(OnSummonHulk);
    }

    private void OnSummonHulk(Entity<SummonRotHulkComponent> ent, ref SummonRotHulkEvent args)
    {
        if (args.Handled)
            return;
        if (_net.IsClient)
            return;

        var uid = ent.Owner;
        var comp = ent.Comp;
        var xform = Transform(uid);

        var nearbyTrash = new List<EntityUid>();
        foreach (var e in _lookup.GetEntitiesInRange(xform.Coordinates, comp.SearchRadius, LookupFlags.Uncontained))
        {
            if (_tags.HasTag(e, comp.TrashTag))
                nearbyTrash.Add(e);
            if (nearbyTrash.Count >= comp.MaxTrash)
                break;
        }

        if (nearbyTrash.Count < comp.MinTrash)
        {
            _popup.PopupEntity(Loc.GetString("wraith-rot-hulk-too-clean"), ent.Owner, ent.Owner);
            return;
        }

        //TO DO: Would be cool if the trash slowly was dragged into a center point and only then spawn the rot hulk in the middle of that, rather than just deleting. For parity's sake and all.
        //Leaving this for part 2, it's just cosmetic.

        foreach (var trash in nearbyTrash)
            QueueDel(trash);

        // Choose which prototype to spawn
        var isBuff = nearbyTrash.Count >= comp.BuffThreshold;
        var proto = isBuff
            ? comp.BuffRotHulkProto
            : comp.RotHulkProto;

        Spawn(proto, xform.Coordinates);
        _popup.PopupEntity(Loc.GetString("wraith-rot-hulk-emerge"), ent.Owner, ent.Owner);
        _admin.Add(LogType.EntitySpawn, LogImpact.Medium, $"{args.Performer} has summoned a rot hulk");

        args.Handled = true;
    }

}
