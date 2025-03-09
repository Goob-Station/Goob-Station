using Content.Server._Shitmed.Objectives.Components;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Database;
using Content.Shared.Mind;
using Robust.Shared.Player;
namespace Content.Server._Shitmed.Objectives.Systems;

public sealed class ForceHereticObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindComponent, ObjectiveAddedEvent>(OnObjectiveAdded);
    }

    private void OnObjectiveAdded(EntityUid uid, MindComponent comp, ref ObjectiveAddedEvent args)
    {
        if (!TryComp<ActorComponent>(comp.CurrentEntity, out var actor))
            return;

        if (HasComp<ForceHereticObjectiveComponent>(args.Objective))
        {
            _antag.ForceMakeAntag<HereticRuleComponent>(actor.PlayerSession, "Heretic");

            _adminLogManager.Add(LogType.Mind,
                LogImpact.Medium,
                $"{ToPrettyString(uid)} has been given heretic status by an antag objective.");
        }
    }
}
