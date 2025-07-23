using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Mind;

namespace Content.Server.Objectives.Systems;

// Automatically assign an objective as complete.

public sealed class AutoCompleteConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoCompleteConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, AutoCompleteConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 1f;
    }
}
