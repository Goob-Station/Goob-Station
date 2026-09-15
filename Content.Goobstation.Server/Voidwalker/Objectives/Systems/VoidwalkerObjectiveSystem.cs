using Content.Goobstation.Server.Voidwalker.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Voidwalker.Objectives.Systems;

public sealed partial class VoidwalkerObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _NumberObjectiveSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Voidwalker.Objectives.Components.VoidwalkerKidnapConditionComponent, ObjectiveGetProgressEvent>(OnKidnapGetProgress);
    }

    private void OnKidnapGetProgress(EntityUid uid, Shared.Voidwalker.Objectives.Components.VoidwalkerKidnapConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _NumberObjectiveSystem.GetTarget(uid);
        args.Progress = target != 0 ? MathF.Min((float) comp.Kidnapped / target, 1f) : 1f; // idek man
    }
}
