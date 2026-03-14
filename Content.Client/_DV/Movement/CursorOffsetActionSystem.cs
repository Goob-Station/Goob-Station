using System.Numerics;
using Content.Shared.Movement.Components; // DeltaV - make EyeCursorOffsetComponent entirely Shared
using Content.Shared._DV.Movement;
using Robust.Client.Timing;

namespace Content.Client._DV.Movement;

public sealed class CursorOffsetActionSystem : SharedCursorOffsetActionSystem
{
    [Dependency] private readonly IClientGameTiming _gameTiming = default!;

    protected override void OnInit(Entity<CursorOffsetActionComponent> ent, ref ComponentInit args)
    {
        base.OnInit(ent, ref args);

        if (!TryComp<EyeCursorOffsetComponent>(ent, out var eyeOffset))
            return;

        eyeOffset.Enabled = ent.Comp.Active;
        eyeOffset.CurrentPosition = Vector2.Zero;
    }

    protected override void OnAction(Entity<CursorOffsetActionComponent> ent, ref CursorOffsetActionEvent args)
    {
        base.OnAction(ent, ref args);

        if (!TryComp<EyeCursorOffsetComponent>(ent, out var eyeOffset))
            return;

        eyeOffset.Enabled = ent.Comp.Active;

        if (_gameTiming.IsFirstTimePredicted)
            eyeOffset.CurrentPosition = Vector2.Zero;
    }
}
