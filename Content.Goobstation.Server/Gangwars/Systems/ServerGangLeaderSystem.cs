using Content.Goobstation.Server.Gangwars.Roles;
using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Server.Antag;
using Content.Server.Roles;
using Content.Shared.Mind;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Gangwars.Systems;

public sealed class ServerGangLeaderSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GangMemberComponent, GangMemberRecruitedEvent>(OnRecruited);
        SubscribeLocalEvent<GangRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void OnGetBriefing(EntityUid uid, GangRoleComponent comp, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;
        var leader = HasComp<GangLeaderComponent>(ent);
        args.Append(Loc.GetString(leader ? "roles-antag-gang-leader-objective" : "roles-antag-gang-objective"));
    }

    private void OnRecruited(EntityUid uid, GangMemberComponent _, GangMemberRecruitedEvent args)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        _antag.SendBriefing(
            actor.PlayerSession,
            Loc.GetString("gang-member-role-greeting"),
            Color.FromHex("#c34e4e"),
            new SoundPathSpecifier("/Audio/_Goobstation/Gangs/gang_start.ogg"));

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
        {
            _mind.TryAddObjective(mindId, mind, "GangFirstPlaceObjective");
            _mind.TryAddObjective(mindId, mind, "GangEarnPointsObjective");
            _mind.TryAddObjective(mindId, mind, "GangMemberSurviveObjective");
        }
    }
}


