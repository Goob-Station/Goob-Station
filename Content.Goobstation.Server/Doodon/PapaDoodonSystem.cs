using Content.Goobstation.Shared.Doodon.Components;
using Content.Server.DoAfter;
using Content.Shared.DoAfter;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Doodon;

public sealed class PapaDoodonSystem : EntitySystem
{
    const string TownhallPrototype = "DoodonTownHall"; // The prototype ID of the townhall to be spawned.
    const int TownhallPlacementTime = 4; // Time in seconds to establish the townhall.

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PapaDoodonComponent, EstablishTownhallTileActionEvent>(OnEstablishTownhall);
        SubscribeLocalEvent<PapaDoodonComponent, EstablishTownhallDoAfterEvent>(OnEstablishTownHallDoAfter);


    }
    private void OnEstablishTownhall(Entity<PapaDoodonComponent> uid, ref EstablishTownhallTileActionEvent args)
    {
        var ev = new EstablishTownhallDoAfterEvent(args.Target);
        var doafter = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(TownhallPlacementTime), ev, args.Performer)
        {
            BreakOnMove = true,
            DistanceThreshold = 2.5f,
            NeedHand = false,
            MultiplyDelay = false
        };
        _doAfter.TryStartDoAfter(doafter);
    }
    private void OnEstablishTownHallDoAfter(Entity<PapaDoodonComponent> papaUid, ref EstablishTownhallDoAfterEvent args)
    {
        if (papaUid.Comp.TownhallPlaced || !TryComp<DoodonComponent>(papaUid, out var doodonComp))
            return;

        var townhall = PredictedSpawnAtPosition(TownhallPrototype, args.Coords);
        if (!TryComp<DoodonTownhallComponent>(townhall, out var townhallComp))
            return;

        papaUid.Comp.TownhallPlaced = true;
        townhallComp.LinkedPapaDoodon = papaUid;
        doodonComp.LinkedTownhall = townhall;

        _popup.PopupPredicted(Loc.GetString("doodon-townhall-established"), papaUid, papaUid);

        // Dirty components
        DirtyField(papaUid, papaUid.Comp, "TownhallPlaced");
        DirtyField(townhall, townhallComp, "LinkedPapaDoodon");
        DirtyField(papaUid, doodonComp, "LinkedTownhall");
    }
}
