using Content.Server._Shitmed.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server._Shitmed.Objectives.Systems;

public sealed class RoleplayObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoleplayObjectiveComponent, ObjectiveGetProgressEvent>(OnRoleplayGetProgress);
    }

    private void OnRoleplayGetProgress(EntityUid uid, RoleplayObjectiveComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 1f;
    }
}
