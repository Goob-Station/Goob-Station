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

    //TO DO: Where do I even start? This system needs a full rework. The wraith is supposed to summon a ritual circle which ocasionally spawns portals that fizzle out once the creature is summoned.
    //The portal is supposed to have a max cap of roughly 5 summoned critters at any given time
    //The portal's creatures are all supposed to have their own unique skills, which the AI is supposed to be capable of using.
    //The wraith is supposed to be able to pick if they want to spawn only one type of mob, or if it should spawn any of them at random (Blame the wiki for lying to us about how this works)
    //When trying to summon a second portal, the wraith is supposed to get a pop up window asking if they want to delete the old portal, as they cannot have two portals at once.
    //All of this will only be done in Wraith part 2, fuck you.
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
