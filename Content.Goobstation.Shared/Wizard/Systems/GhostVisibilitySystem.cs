using Content.Goobstation.Common.Wizard.Events;
using Content.Goobstation.Shared.Wizard.Rules;
using Content.Shared.Administration.Managers;
using Content.Shared.Ghost;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class GhostVisibilitySystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminManager _adminManager = default!;

    private EntityQuery<GhostComponent> _ghostQuery;

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();

        SubscribeLocalEvent<GetDeadchatAdditionalHearersEvent>(OnGetDeadchatHearers);
        SubscribeLocalEvent<GetCanSeeGhostsEvent>(OnGetCanSeeGhosts);
    }

    private void OnGetDeadchatHearers(ref GetDeadchatAdditionalHearersEvent ev)
    {
        if (IsRuleActive())
            ev.Filter = Filter.Broadcast();
    }
    private void OnGetCanSeeGhosts(ref GetCanSeeGhostsEvent ev)
    {
        if (CanSeeGhosts(ev.Uid, ev.CheckIfForced))
            ev.Can = true;
    }

    public bool CanSeeGhosts(EntityUid? uid, bool checkIfForced)
    {
        if (IsRuleActive() ||
            uid is { Valid: true } && (checkIfForced && _ghostQuery.HasComp(uid) || _adminManager.IsAdmin(uid.Value)))
            return true;
        return false;
    }

    public bool IsRuleActive()
    {
        var query = EntityQueryEnumerator<GhostsVisibleRuleComponent>();
        while (query.MoveNext(out _))
            return true;

        return false;
    }
}