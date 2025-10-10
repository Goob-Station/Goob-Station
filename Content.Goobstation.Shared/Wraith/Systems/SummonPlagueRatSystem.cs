using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class SummonPlagueRatSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonPlagueRatComponent, SummonPlagueRatEvent>(OnSummonRat);
    }

    // Part 2 TO DO: Only allow one summoned plaguerat to exist on station at any given time.

    private void OnSummonRat(Entity<SummonPlagueRatComponent> ent, ref SummonPlagueRatEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var xform = Transform(uid);

        if (args.Handled)
            return;

        if (_physics.GetEntitiesIntersectingBody(ent.Owner, (int) CollisionGroup.Impassable).Count > 0)
        {
            _popup.PopupPredicted(Loc.GetString("wraith-plaguerat-blocked"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        if (_net.IsServer)
        {
            var voidUid = Spawn(comp.RatProto, xform.Coordinates);
            _popup.PopupPredicted(Loc.GetString("wraith-plaguerat-channel"), uid, uid);
        }

        args.Handled = true;
    }
}
