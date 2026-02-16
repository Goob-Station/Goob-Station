using Content.Server.Ghost;
using Content.Server.Objectives.Components;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Mind;
using Content.Shared.Mind.Components; // goob - fix teach a lesson
using Content.Shared.Mobs;
using Content.Shared.Objectives.Components;

namespace Content.Server._Starlight.Objectives;

/// <summary>
/// Handles Teach a Lesson logic on if a specific entity has died at least once during the round
/// </summary>
public sealed class TeachALessonConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeachALessonTargetComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<TeachALessonTargetComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<TeachALessonTargetComponent, MindRemovedMessage>(OnMindRemoved); // goob - fix teach a lesson
        SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhostAttempt); // goob - fix teach a lesson
        SubscribeLocalEvent<TeachALessonConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<TeachALessonConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<TeachALessonConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = ent.Comp.HasDied ? 1.0f : 0.0f;
    }

    private void OnAfterAssign(Entity<TeachALessonConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (!TryComp(ent.Owner, out TargetObjectiveComponent? targetObjective))
            return;
        var targetMindUid = targetObjective.Target;
        if (targetMindUid is null)
            return;
        if (!TryComp(targetMindUid, out MindComponent? targetMind))
            return;
        var targetMobUid = targetMind.CurrentEntity;
        if (targetMobUid is null)
            return;
        var targetComponent = EnsureComp<TeachALessonTargetComponent>(targetMobUid.Value);
        targetComponent.Teachers.Add(ent.Owner);
    }

    private void OnMindAdded(EntityUid uid, TeachALessonTargetComponent component, MindAddedMessage args) // goob - fix teach a lesson
    {
        var targetComponent = EnsureComp<TeachALessonTargetComponent>(args.Container.Owner);
        foreach (var teacher in component.Teachers)
        {
            targetComponent.Teachers.Add(teacher);
        }
    }

    private void OnMindRemoved(EntityUid uid, TeachALessonTargetComponent component, MindRemovedMessage args) // goob - fix teach a lesson
    {
        // cryo storage fix godo
        if (TryComp<CryostorageContainedComponent>(uid, out var contained) && contained.GracePeriodEndTime == null)
        {
            TriggerObjective(component);
        }

        RemCompDeferred<TeachALessonTargetComponent>(uid);
    }

    private void OnGhostAttempt(GhostAttemptHandleEvent args) // goob - fix teach a lesson
    {
        if (args.Mind.OwnedEntity is not { } owned || !TryComp<TeachALessonTargetComponent>(owned, out var target))
            return;

        if (args.CanReturnGlobal && args.Mind.VisitingEntity == null)
        {
            TriggerObjective(target);
            return;
        }

        if (args.CanReturnGlobal)
            return;

        TriggerObjective(target);
    }

    private void OnMobStateChanged(Entity<TeachALessonTargetComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        TriggerObjective(ent.Comp);
    }

    private void TriggerObjective(TeachALessonTargetComponent component) // goob - fix teach a lesson
    {
        foreach (var teacher in component.Teachers)
        {
            if (!TryComp(teacher, out TeachALessonConditionComponent? condition))
                continue;

            condition.HasDied = true;
        }
    }
}
