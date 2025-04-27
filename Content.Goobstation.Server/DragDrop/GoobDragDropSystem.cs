using Content.Goobstation.Shared.DragDrop;
using Content.Server.Construction.Components;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.DragDrop;

public sealed partial class GoobDragDropSystem : SharedGoobDragDropSystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructionComponent, DragDropTargetEvent>(OnDragDropConstruction);
    }

    // this is cursed but making construction system code handle DragDropTargetEvent would be even more cursed
    // if it works it works
    private void OnDragDropConstruction(EntityUid uid, ConstructionComponent comp, ref DragDropTargetEvent args)
    {
        var ev = new InteractUsingEvent(args.User, args.Dragged, uid, Transform(uid).Coordinates);
        RaiseLocalEvent(uid, ev);
    }
}
