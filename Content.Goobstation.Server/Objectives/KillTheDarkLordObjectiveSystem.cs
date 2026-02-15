using Content.Goobstation.Shared.DarkLord;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Objectives;

/// <summary>
/// This handles the "kill the dark lord" objective for the chosen one
/// </summary>
public sealed class KillTheDarkLordObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<KillTheDarkLordObjectiveComponent, ObjectiveGetProgressEvent>(OnGetDarkLordKillProgress);
    }
    private void OnGetDarkLordKillProgress(EntityUid uid, KillTheDarkLordObjectiveComponent component, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0f;
        var query = EntityQueryEnumerator<DarkLordMarkerComponent>();
        while (query.MoveNext(out var darkLordUid, out _))
        {
            if (!_mind.TryGetMind(darkLordUid, out _, out var mindId))
            {
                args.Progress = 1f;
                return;
            }

            args.Progress = _mind.IsCharacterDeadIc(mindId) ? 1f : 0f;
            return;
        }

        args.Progress = 1f;
    }
}
