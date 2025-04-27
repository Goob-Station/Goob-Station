using Content.Client.Construction;
using Content.Goobstation.Client.Construction;
using Content.Goobstation.Shared.DragDrop;
using Content.Shared.DragDrop;
using Content.Shared.Interaction;
using Content.Shared.Item;

namespace Content.Goobstation.Client.DragDrop;

public sealed partial class GoobDragDropSystem : SharedGoobDragDropSystem
{
    [Dependency] private readonly ConstructionSystem _construction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructionComponent, DragDropTargetEvent>(OnDragDropConstruction);
        SubscribeLocalEvent<ConstructionComponent, CanDropTargetEvent>(CanDropTargetConstruction);

        SubscribeLocalEvent<ConstructionGhostComponent, DragDropTargetEvent>(OnDragDrop);
        SubscribeLocalEvent<ConstructionGhostComponent, CanDropTargetEvent>(CanDropTarget);
    }

    // this is cursed but making construction system code handle DragDropTargetEvent would be even more cursed
    // if it works it works
    private void OnDragDropConstruction(EntityUid uid, ConstructionComponent comp, ref DragDropTargetEvent args)
    {
        var ev = new InteractUsingEvent(args.User, args.Dragged, uid, Transform(uid).Coordinates);
        RaiseLocalEvent(uid, ev);
    }

    private void CanDropTargetConstruction(EntityUid uid, ConstructionComponent comp, ref CanDropTargetEvent args)
    {
        if (HasComp<ItemComponent>(args.Dragged))
        {
            args.CanDrop = true;
            args.Handled = true;
        }
    }

    private void OnDragDrop(Entity<ConstructionGhostComponent> ent, ref DragDropTargetEvent args)
    {
        _construction.TryStartConstruction(ent, ent.Comp, args.Dragged);
    }

    private void CanDropTarget(Entity<ConstructionGhostComponent> ent, ref CanDropTargetEvent args)
    {
        if (HasComp<ItemComponent>(args.Dragged))
        {
            args.CanDrop = true;
            args.Handled = true;
        }
    }
}
