using Content.Goobstation.Server.Devil.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Devil.Objectives.Systems;

public sealed partial class DevilObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignContractConditionComponent, ObjectiveGetProgressEvent>(OnContractGetProgress);
    }

    private void OnContractGetProgress(EntityUid uid, SignContractConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        args.Progress = target != 0 ? MathF.Min(comp.ContractsSigned / target, 1f) : 1f;
    }
}
