using Content.Shared._DV.Movement;
using Content.Shared.Camera;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server._DV.Movement;

public sealed class CursorOffsetActionSystem : SharedCursorOffsetActionSystem
{
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursorOffsetActionComponent, GetEyePvsScaleEvent>(OnGetEyePvsScale);
    }

    protected override void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        base.OnAction(ent, ref args);

        _eye.UpdatePvsScale(args.Performer);
    }

    private void OnGetEyePvsScale(Entity<CursorOffsetActionComponent> entity,
        ref GetEyePvsScaleEvent args)
    {
        if (!TryComp(entity, out EyeCursorOffsetComponent? eyeCursorOffset) || !entity.Comp.Active)
            return;

        args.Scale += eyeCursorOffset.PvsIncrease;
    }
}
