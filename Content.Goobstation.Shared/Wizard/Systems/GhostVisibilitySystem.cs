using Content.Goobstation.Common.Wizard.Events;
using Content.Shared._Goobstation.Wizard.EventSpells;
using Content.Shared.Administration.Managers;
using Content.Shared.Ghost;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

public abstract class SharedGhostVisibilitySystem : EntitySystem
{
    [Dependency] private EntityQuery<GhostComponent> _ghostQuery = default!;
    [Dependency] private ISharedAdminManager _adminManager = default!;

    protected static readonly EntProtoId GameRule = "GhostsVisible";

    public override void Initialize()
    {
        base.Initialize();

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