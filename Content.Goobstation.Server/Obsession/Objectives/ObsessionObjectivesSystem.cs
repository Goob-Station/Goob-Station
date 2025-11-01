using Content.Goobstation.Shared.Obsession;
using Content.Server.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Obsession;

public sealed class ObsessionObjectivesSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObsessionInteractConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<ObsessionInteractConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<ObsessionInteractConditionComponent, RefreshObsessionObjectiveStatsEvent>(OnRefresh);

        SubscribeLocalEvent<ObsessionKillConditionComponent, ObjectiveAfterAssignEvent>(OnAfterKillAssign);
        SubscribeLocalEvent<ObsessionKillConditionComponent, ObjectiveGetProgressEvent>(OnGetKillProgress);
        SubscribeLocalEvent<ObsessionKillConditionComponent, ObsessionTargetDiedEvent>(OnKillTargetDied);
    }

    private void OnAfterAssign(EntityUid uid, ObsessionInteractConditionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        comp.Required = comp.Limits.Next(_random);

        if (TryComp<ObsessedComponent>(args.Mind.CurrentEntity, out var obsessed))
        {
            obsessed.Interactions[comp.Interaction] = 0;
            _meta.SetEntityName(uid, Loc.GetString(comp.Name));
            _meta.SetEntityDescription(uid, Loc.GetString(comp.Desc, ("target", obsessed.TargetName), ("count", comp.Required)));
        }
    }

    private void OnGetProgress(EntityUid uid, ObsessionInteractConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (!args.Mind.CurrentEntity.HasValue)
            return;

        args.Progress = comp.LockedState ?? GetProgress(args.Mind.CurrentEntity.Value, comp);
    }

    private void OnRefresh(EntityUid uid, ObsessionInteractConditionComponent comp, ref RefreshObsessionObjectiveStatsEvent args)
    {
        if (comp.Interaction != args.Interaction || comp.LockedState != null)
            return;

        if (args.Count >= comp.Required && comp.NextObjective.HasValue)
        {
            comp.LockedState = 1f;
            _mind.TryAddObjective(args.MindId, args.Mind, comp.NextObjective);
        }
    }

    private void OnAfterKillAssign(EntityUid uid, ObsessionKillConditionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        if (TryComp<ObsessedComponent>(args.Mind.CurrentEntity, out var obsessed))
        {
            _meta.SetEntityName(uid, Loc.GetString(comp.Name, ("target", obsessed.TargetName)));
            _meta.SetEntityDescription(uid, Loc.GetString(comp.Desc, ("target", obsessed.TargetName)));
        }
    }

    private void OnGetKillProgress(EntityUid uid, ObsessionKillConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = comp.Success ? 1f : 0f;
    }

    private void OnKillTargetDied(EntityUid uid, ObsessionKillConditionComponent comp, ref ObsessionTargetDiedEvent args)
    {
        args.Handled = true;

        if (args.Mind.CurrentEntity.HasValue)
            comp.Success = true;
    }

    private float GetProgress(EntityUid self, ObsessionInteractConditionComponent comp)
    {
        if (!TryComp<ObsessedComponent>(self, out var obsessed))
            return 0f;

        return Math.Clamp((float) obsessed.Interactions[comp.Interaction] / (float) comp.Required, 0f, 1f);
    }
}
