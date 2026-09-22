using Content.Goobstation.Shared.Voidwalker.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Voidwalker.Objectives.Systems;

public sealed partial class VoidwalkerObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _numberObjectiveSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidwalkerKidnapConditionComponent, ObjectiveGetProgressEvent>(OnKidnapGetProgress);
    }

    private void OnKidnapGetProgress(EntityUid uid, VoidwalkerKidnapConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _numberObjectiveSystem.GetTarget(uid);
        args.Progress = target != 0 ? MathF.Min((float) comp.Kidnapped / target, 1f) : 1f; // idek man
    }
}
