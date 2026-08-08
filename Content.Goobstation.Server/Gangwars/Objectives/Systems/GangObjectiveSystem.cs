using System.Linq;
using Content.Goobstation.Server.Gangwars.Objectives.Components;
using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Systems;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Gangwars.Objectives.Systems;

public sealed class GangObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly GangwarRuleSystem _gangwarRule = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangLeaderRecruitConditionComponent, ObjectiveGetProgressEvent>(OnRecruitGetProgress);
        SubscribeLocalEvent<GangFirstPlaceConditionComponent, ObjectiveGetProgressEvent>(OnFirstPlaceGetProgress);
        SubscribeLocalEvent<GangEarnPointsConditionComponent, ObjectiveGetProgressEvent>(OnEarnPointsGetProgress);
    }

    private void OnRecruitGetProgress(EntityUid uid, GangLeaderRecruitConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);

        if (TryComp<MindComponent>(args.MindId, out var mind)
            && mind.OwnedEntity is { } mob
            && TryComp<GangLeaderComponent>(mob, out var leader))
            comp.Recruited = leader.InvitesUsed;

        args.Progress = target > 0 ? MathF.Min((float) comp.Recruited / target, 1f) : 1f;
    }

    private void OnFirstPlaceGetProgress(EntityUid uid, GangFirstPlaceConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0f;

        if (!TryComp<MindComponent>(args.MindId, out var mind)
            || mind.OwnedEntity is not { } mob
            || !TryComp<GangMemberComponent>(mob, out var member)
            || member.Gang is not { } myColor)
            return;

        var scores = _gangwarRule.CalculateScores();

        if (scores.Count == 0)
            return;

        var topEntry = scores.Aggregate((a, b) => a.Value >= b.Value ? a : b);

        if (topEntry.Key == myColor)
            args.Progress = 1f;
    }

    private void OnEarnPointsGetProgress(EntityUid uid, GangEarnPointsConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);

        if (!TryComp<MindComponent>(args.MindId, out var mind)
            || mind.OwnedEntity is not { } mob
            || !TryComp<GangMemberComponent>(mob, out var member))
        {
            args.Progress = 0f;
            return;
        }

        args.Progress = target > 0 ? MathF.Min((float) member.GangPoints / target, 1f) : 1f;
    }
}
