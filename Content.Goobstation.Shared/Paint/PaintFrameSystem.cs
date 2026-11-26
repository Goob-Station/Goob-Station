using Content.Shared.Interaction;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Paint;

public sealed partial class PaintFrameSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaintCanvasVisualsComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<PaintCanvasVisualsComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !HasComp<PaintingFrameComponent>(args.Used))
            return;

        args.Handled = TryFrame(ent.Owner, args.Used, args.User);
    }

    public bool TryFrame(Entity<PaintCanvasVisualsComponent?> painting, EntityUid frameUid, EntityUid user)
    {
        if (!Resolve(painting, ref painting.Comp, false) || !TryComp<PaintingFrameComponent>(frameUid, out var frame))
            return false;

        if (!TryComp<PaintSurfaceComponent>(painting, out var surface) || !surface.Finished)
        {
            _popup.PopupClient(Loc.GetString("paint-frame-unfinished"), painting, user);
            return true;
        }

        if (painting.Comp.Frame != null)
        {
            _popup.PopupClient(Loc.GetString("paint-frame-already-framed"), painting, user);
            return true;
        }

        painting.Comp.Frame = frame.Material;
        Dirty(painting);
        _popup.PopupClient(Loc.GetString("paint-frame-applied", ("painting", painting.Owner)), painting, user);
        PredictedQueueDel(frameUid);
        return true;
    }
}
