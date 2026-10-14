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
        args.Progress = KidnapProgress(comp, _numberObjectiveSystem.GetTarget(uid));
    }

    private float KidnapProgress(VoidwalkerKidnapConditionComponent comp, int target)
    {
        if (target == 0)
            return 1f;

        return MathF.Min(comp.Kidnapped / (float) target, 1f);
    }
}
