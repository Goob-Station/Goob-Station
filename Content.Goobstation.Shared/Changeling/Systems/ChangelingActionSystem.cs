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
        var user = args.User;

        if (args.Cancelled)
            return;

        if (!_changelingQuery.TryComp(user, out var changelingComp))
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-action-not-changeling"), user, user);
            args.Cancelled = true;
            return;
        }

        if (action.Comp.BlockedByFire && CheckFireStatus(user))
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-onfire"), user, user, PopupType.LargeCaution);
            args.Cancelled = true;
            return;
        }

        // TODO: Change this shit to something good
        if (!action.Comp.UseInLesserForm && changelingComp.IsInLesserForm || !action.Comp.UseInLastResort && changelingComp.IsInLastResort)
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-action-fail-lesserform"), user, user);
            args.Cancelled = true;
            return;
        }

        if (changelingComp.Chemicals < action.Comp.ChemicalCost)
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-chemicals-deficit"), user, user);
            args.Cancelled = true;
            return;
        }

        if (changelingComp.TotalAbsorbedEntities < action.Comp.RequireAbsorbed)
        {
            var delta = action.Comp.RequireAbsorbed - changelingComp.TotalAbsorbedEntities;

            _popupSystem.PopupClient(Loc.GetString("changeling-action-fail-absorbed", ("number", delta)), user, user);
            args.Cancelled = true;
            return;
        }

        // Update chemicals

    }

    private bool CheckFireStatus(EntityUid uid) => HasComp<OnFireComponent>(uid);
}
