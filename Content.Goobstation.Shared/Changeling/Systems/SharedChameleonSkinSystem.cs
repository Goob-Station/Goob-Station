using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed partial class SharedChameleonSkinSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedChangelingChemicalSystem _lingChem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    private EntityQuery<ChangelingChemicalComponent> _chemQuery;
    private EntityQuery<DarknessAdaptionComponent> _darkadaptQuery;
    private EntityQuery<StealthComponent> _stealthQuery;

    public override void Initialize()
    {
        base.Initialize();

        _chemQuery = GetEntityQuery<ChangelingChemicalComponent>();
        _darkadaptQuery = GetEntityQuery<DarknessAdaptionComponent>();
        _stealthQuery = GetEntityQuery<StealthComponent>();

        SubscribeLocalEvent<ChameleonSkinComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChameleonSkinComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ChameleonSkinComponent, ActionChameleonSkinEvent>(OnToggleAbility);
        SubscribeLocalEvent<ChameleonSkinComponent, IgnitedEvent>(OnIgnite);
    }

    private void OnMapInit(Entity<ChameleonSkinComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ActionEnt = _actions.AddAction(ent, ent.Comp.ActionId);

        Dirty(ent);
    }

    private void OnShutdown(Entity<ChameleonSkinComponent> ent, ref ComponentShutdown args)
    {
        RemoveStealth(ent);

        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    #region Event Handlers

    private void OnToggleAbility(Entity<ChameleonSkinComponent> ent, ref ActionChameleonSkinEvent args)
    {
        // cancel and refund if darkness adaption is active (prevents issues)
        if (_chemQuery.TryComp(ent, out var chem)
            && _darkadaptQuery.TryComp(ent, out var dark)
            && dark.Active)
        {
            _lingChem.UpdateChemicals((ent, chem), Comp<ChangelingActionComponent>(args.Action).ChemicalCost);
            return;
        }

        if (FireInvalidCheck(ent))
        {
            DoPopup(ent, ent.Comp.OnFirePopup, PopupType.LargeCaution);
            return;
        }

        ent.Comp.Active = !ent.Comp.Active;
        var popup = ent.Comp.Active ? ent.Comp.ActivePopup : ent.Comp.InactivePopup;

        if (ent.Comp.Active)
            EnsureStealth(ent);
        else
            RemoveStealth(ent);

        Dirty(ent);

        DoPopup(ent, popup);
    }

    private void OnIgnite(Entity<ChameleonSkinComponent> ent, ref IgnitedEvent args)
    {
        if (!_stealthQuery.TryComp(ent, out var stealth))
            return;

        if (ent.Comp.Active)
            DoPopup(ent, ent.Comp.IgnitedPopup, PopupType.LargeCaution);

        ent.Comp.Active = false;
        _stealth.SetEnabled(ent, false, stealth);

        Dirty(ent);
    }
    #endregion

    #region Helper Methods

    private void EnsureStealth(Entity<ChameleonSkinComponent> ent)
    {
        EnsureComp<StealthComponent>(ent, out var stealth);
        _stealth.SetEnabled(ent, ent.Comp.Active, stealth);
        stealth.RevealOnAttack = ent.Comp.RevealOnAttack;
        stealth.RevealOnDamage = ent.Comp.RevealOnDamage;

        EnsureComp<StealthOnMoveComponent>(ent, out var stealthMove);
        stealthMove.NoMoveTime = ent.Comp.WaitTime;
        stealthMove.PassiveVisibilityRate = ent.Comp.VisibilityRate;
        stealthMove.BreakOnMove = ent.Comp.BreakOnMove;

        Dirty(ent, stealth);
        Dirty(ent, stealthMove);
    }

    private void RemoveStealth(Entity<ChameleonSkinComponent> ent)
    {
        RemComp<StealthComponent>(ent);
        RemComp<StealthOnMoveComponent>(ent);
    }

    private bool FireInvalidCheck(Entity<ChameleonSkinComponent> ent)
    {
        var fireEv = new GetFireStateEvent();
        RaiseLocalEvent(ent, ref fireEv);

        return fireEv.OnFire;
    }

    private void DoPopup(Entity<ChameleonSkinComponent> ent, LocId popup, PopupType popupType = PopupType.Small)
    {
        _popup.PopupClient(Loc.GetString(popup), ent, ent, popupType);
    }
    #endregion
}
