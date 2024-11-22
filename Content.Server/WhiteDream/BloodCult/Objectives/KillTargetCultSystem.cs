using System.Linq;
using Content.Server.Roles.Jobs;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Server.WhiteDream.BloodCult.Objectives;

public sealed class KillTargetCultSystem : EntitySystem
{
    [Dependency] private readonly JobSystem _job = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<KillTargetCultComponent, ObjectiveAssignedEvent>(OnObjectiveAssigned);
        SubscribeLocalEvent<KillTargetCultComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<KillTargetCultComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnObjectiveAssigned(Entity<KillTargetCultComponent> ent, ref ObjectiveAssignedEvent args)
    {
        var cultistRule = EntityManager.EntityQuery<BloodCultRuleComponent>().FirstOrDefault();
        if (cultistRule?.OfferingTarget is { } target)
            ent.Comp.Target = target;
    }

    private void OnAfterAssign(Entity<KillTargetCultComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(ent, GetTitle(ent.Comp.Target, ent.Comp.Title), args.Meta);
    }

    private void OnGetProgress(Entity<KillTargetCultComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = ent.Comp.Target;

        args.Progress = !HasComp<MobStateComponent>(target) || _mobState.IsDead(target)
            ? args.Progress = 1f
            : args.Progress = 0f;
    }

    private string GetTitle(EntityUid target, string title)
    {
        var targetName = MetaData(target).EntityName;
        var jobName = _job.MindTryGetJobName(target);
        return Loc.GetString(title, ("targetName", targetName), ("job", jobName));
    }
}
