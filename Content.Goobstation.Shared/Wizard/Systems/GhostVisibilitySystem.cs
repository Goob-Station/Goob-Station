using Content.Goobstation.Common.Wizard.Components;
using Content.Goobstation.Common.Wizard.Events;
using Content.Goobstation.Shared.Wizard.Rules;
using Content.Shared.Administration.Managers;
using Content.Shared.Ghost;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Wizard.Systems;

/// <summary>
/// Checks whether an entity should be allowed to see ghosts.
/// </summary>
public sealed partial class GhostVisibilitySystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminManager _adminManager = default!;

    private EntityQuery<GhostComponent> _ghostQuery;
    private EntityQuery<ScryingViewerComponent> _scryingViewerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();
        _scryingViewerQuery = GetEntityQuery<ScryingViewerComponent>();

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
        if (CanSeeGhosts(ev.Uid))
            ev.Can = true;
    }

    public bool CanSeeGhosts(EntityUid? uid)
        => IsRuleActive() ||
        uid is { Valid: true } && (
            _ghostQuery.HasComp(uid) ||
            _scryingViewerQuery.HasComp(uid) ||
            _adminManager.IsAdmin(uid.Value)
        );

    public bool IsRuleActive()
    {
        var query = EntityQueryEnumerator<GhostsVisibleRuleComponent>();
        while (query.MoveNext(out _))
            return true;

        return false;
    }
}