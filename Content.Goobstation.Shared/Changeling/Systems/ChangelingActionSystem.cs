using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed class ChangelingActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private EntityQuery<ChangelingIdentityComponent> _changelingQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingActionComponent, ActionAttemptEvent>(OnChangelingActionAttempt);

        _changelingQuery = GetEntityQuery<ChangelingIdentityComponent>();
    }

    private void OnChangelingActionAttempt(Entity<ChangelingActionComponent> action, ref ActionAttemptEvent args)
    {
        var user = args.User;)

        if (!_changelingQuery.TryComp(user, out var changelingComp))
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-action-not-changeling"), user, user);
            return;
        }

        if (action.Comp.BlockedByFire && CheckFireStatus(user))
            return;
    }

    private bool CheckFireStatus(EntityUid uid) => HasComp<OnFireComponent>(uid);
}
