using Content.Goobstation.Shared.Slasher.Objectives;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Slasher.Objectives;

/// <summary>
/// Handles Slasher-specific objectives.
/// </summary>
public sealed class SlasherObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherAbsorbSoulsConditionComponent, ObjectiveGetProgressEvent>(OnGetAbsorbSoulsProgress);
    }

    private void OnGetAbsorbSoulsProgress(EntityUid uid, SlasherAbsorbSoulsConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = _number.GetTarget(uid) != 0 ? MathF.Min(comp.Absorbed / (float) _number.GetTarget(uid), 1f) : 1f;
    }
}
