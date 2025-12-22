using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Atmos.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed class SharedChanglingActionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingActionComponent, ActionAttemptEvent>(OnActionAttempt);

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
    }

    private void OnActionAttempt(Entity<ChangelingActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_lingQuery.TryComp(args.User, out var ling))
            return;

        if (!ent.Comp.UseOnFire && OnFire(args.User))
        {
            DoPopup(args.User, ent.Comp.OnFirePopup, PopupType.LargeCaution);
            args.Cancelled = true;

            return;
        }

        if (!ent.Comp.UseInLastResort && ling.IsInLastResort
            || !ent.Comp.UseInLesserForm && ling.IsInLesserForm)
        {
            DoPopup(args.User, ent.Comp.LesserFormPopup);
            args.Cancelled = true;

            return;
        }

        if (ent.Comp.ChemicalCost > ling.Chemicals)
        {
            DoPopup(args.User, ent.Comp.InvalidChemicalsPopup);
            args.Cancelled = true;

            return;
        }

        if (ent.Comp.RequireAbsorbed > ling.TotalAbsorbedEntities)
        {
            var delta = ent.Comp.RequireAbsorbed - ling.TotalAbsorbedEntities;
            var popup = Loc.GetString("changeling-action-fail-absorbed", ("number", delta));

            DoPopup(args.User, popup);
            args.Cancelled = true;

            return;
        }

        // ideally this should be done via an event.
        var chemicals = ling.Chemicals;

        chemicals -= ent.Comp.ChemicalCost;
        ling.Chemicals = Math.Clamp(chemicals, 0, ling.MaxChemicals);

        Dirty(args.User, ling);
    }

    #region Helper Methods
    private void DoPopup(EntityUid user, LocId popup, PopupType popupType = PopupType.Small)
    {
        _popup.PopupClient(Loc.GetString(popup), user, user, popupType);
    }

    private void DoPopup(EntityUid user, string popup, PopupType popupType = PopupType.Small)
    {
        _popup.PopupClient(popup, user, user, popupType);
    }

    private bool OnFire(EntityUid user)
    {
        return HasComp<OnFireComponent>(user);
    }

    #endregion
}
