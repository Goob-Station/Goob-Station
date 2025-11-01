using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Turrets;

namespace Content.Goobstation.Shared.Grab;

public sealed class GrabModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RaiseGrabModifierEventEvent>(OnRaise);
        SubscribeLocalEvent<GrabModifierComponent, GrabModifierEvent>(ModifyGrab);
        SubscribeLocalEvent<GrabModifierComponent, InventoryRelayedEvent<GrabModifierEvent>>(ModifyInventoryGrab);
        SubscribeLocalEvent<GrabModifierComponent, HeldRelayedEvent<GrabModifierEvent>>(ModifyHeldGrab);
    }

    private void OnRaise(ref RaiseGrabModifierEventEvent args)
    {
        var ev = new GrabModifierEvent(args.User, (GrabStage) args.Stage);
        RaiseLocalEvent(args.User, ref ev);
        args.NewStage = (int?) ev.NewStage;
        args.Modifier += ev.Modifier;
        args.Multiplier *= ev.Multiplier;
        args.SpeedMultiplier *= ev.SpeedMultiplier;
    }

    private void ModifyHeldGrab(Entity<GrabModifierComponent> ent, ref HeldRelayedEvent<GrabModifierEvent> args)
    {
        ModifyGrab(ent, ref args.Args);
    }

    private void ModifyInventoryGrab(Entity<GrabModifierComponent> ent,
        ref InventoryRelayedEvent<GrabModifierEvent> args)
    {
        ModifyGrab(ent, ref args.Args);
    }

    private void ModifyGrab(Entity<GrabModifierComponent> ent, ref GrabModifierEvent args)
    {
        var ev = new FindGrabbingItemEvent();
        RaiseLocalEvent(args.User, ref ev);
        if (ev.GrabbingItem != null && ev.GrabbingItem != ent)
            return;

        var stage = args.Stage;
        if (args.NewStage != null)
            stage = args.NewStage.Value;

        if (stage != GrabStage.No && stage < ent.Comp.StartingGrabStage)
            args.NewStage = ent.Comp.StartingGrabStage;

        args.Multiplier *= ent.Comp.GrabEscapeMultiplier;
        args.Modifier += ent.Comp.GrabEscapeModifier;
        args.SpeedMultiplier *= ent.Comp.GrabMoveSpeedMultiplier;
    }
}
